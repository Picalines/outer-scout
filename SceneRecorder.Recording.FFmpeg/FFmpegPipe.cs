using System.Diagnostics;
using Unity.Collections;

// Modified from: https://github.com/keijiro/FFmpegOut

namespace SceneRecorder.Recording.FFmpeg;

internal sealed class FFmpegPipe : IDisposable
{
    public event Action<string>? OutputReceived;

    private readonly Process _FFmpegProcess;

    private bool _threadsAreTerminated;
    private readonly Thread _CopyThread;
    private readonly Thread _PipeThread;

    private readonly AutoResetEvent _CopyStartEvent = new(initialState: false);
    private readonly AutoResetEvent _CopyEndEvent = new(initialState: false);
    private readonly AutoResetEvent _PipeStartEvent = new(initialState: false);
    private readonly AutoResetEvent _PipeEndEvent = new(initialState: false);

    private readonly Queue<NativeArray<byte>> _CopyQueue = new();
    private readonly Queue<byte[]> _PipeQueue = new();
    private readonly Queue<byte[]> _FreeBuffer = new();

    public FFmpegPipe(string ffmpegPath, string ffmpegArguments)
    {
        _FFmpegProcess = Process.Start(
            new ProcessStartInfo()
            {
                FileName = ffmpegPath,
                Arguments = ffmpegArguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
            }
        );

        _FFmpegProcess.ErrorDataReceived += (sender, args) => OutputReceived?.Invoke(args.Data);
        _FFmpegProcess.BeginErrorReadLine();

        _CopyThread = new Thread(CopyThread);
        _PipeThread = new Thread(PipeThread);
        _CopyThread.Start();
        _PipeThread.Start();
    }

    public bool IsClosed
    {
        get => _threadsAreTerminated;
    }

    public void PushFrameData(NativeArray<byte> data)
    {
        lock (_CopyQueue)
        {
            _CopyQueue.Enqueue(data);
        }

        _CopyStartEvent.Set();
    }

    public void SyncFrameData()
    {
        while (_CopyQueue.Count > 0)
        {
            _CopyEndEvent.WaitOne();
        }

        while (_PipeQueue.Count > 4)
        {
            _PipeEndEvent.WaitOne();
        }
    }

    public void Close()
    {
        if (_threadsAreTerminated)
        {
            return;
        }

        _threadsAreTerminated = true;

        _CopyStartEvent.Set();
        _PipeStartEvent.Set();

        _CopyThread.Join();
        _PipeThread.Join();

        _FFmpegProcess.StandardInput.Close();
        _FFmpegProcess.WaitForExit();

        _FFmpegProcess.Close();
        _FFmpegProcess.Dispose();
    }

    public void Dispose()
    {
        Close();
    }

    ~FFmpegPipe()
    {
        if (_threadsAreTerminated is false)
        {
            throw new InvalidOperationException("ffmpeg pipe closed before work finished");
        }
    }

    private void CopyThread()
    {
        while (_threadsAreTerminated is false)
        {
            _CopyStartEvent.WaitOne();

            while (_CopyQueue.Count > 0)
            {
                NativeArray<byte> source;
                lock (_CopyQueue)
                {
                    source = _CopyQueue.Peek();
                }

                byte[]? buffer = null;
                if (_FreeBuffer.Count > 0)
                {
                    lock (_FreeBuffer)
                    {
                        buffer = _FreeBuffer.Dequeue();
                    }
                }

                if ((buffer?.Length ?? -1) == source.Length)
                {
                    source.CopyTo(buffer);
                }
                else
                {
                    buffer = source.ToArray();
                }

                lock (_PipeQueue)
                {
                    _PipeQueue.Enqueue(buffer!);
                }

                _PipeStartEvent.Set();

                lock (_CopyQueue)
                {
                    _CopyQueue.Dequeue();
                }

                _CopyEndEvent.Set();
            }
        }
    }

    private void PipeThread()
    {
        var ffmpegInputStream = _FFmpegProcess.StandardInput.BaseStream;

        while (_threadsAreTerminated is false)
        {
            _PipeStartEvent.WaitOne();

            while (_PipeQueue.Count > 0)
            {
                byte[] buffer;
                lock (_PipeQueue)
                {
                    buffer = _PipeQueue.Dequeue();
                }

                ffmpegInputStream.Write(buffer, 0, buffer.Length);
                ffmpegInputStream.Flush();

                lock (_FreeBuffer)
                {
                    _FreeBuffer.Enqueue(buffer);
                }

                _PipeEndEvent.Set();
            }
        }
    }
}
