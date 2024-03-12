using System.Diagnostics;
using Unity.Collections;

// Modified from: https://github.com/keijiro/FFmpegOut

namespace OuterScout.Application.FFmpeg;

internal sealed class FFmpegPipe : IDisposable
{
    public event Action<string>? OutputReceived;

    private readonly Process _ffmpegProcess;

    private bool _threadsAreTerminated;
    private readonly Thread _copyThread;
    private readonly Thread _pipeThread;

    private readonly AutoResetEvent _copyStartEvent = new(initialState: false);
    private readonly AutoResetEvent _copyEndEvent = new(initialState: false);
    private readonly AutoResetEvent _pipeStartEvent = new(initialState: false);
    private readonly AutoResetEvent _pipeEndEvent = new(initialState: false);

    private readonly Queue<NativeArray<byte>> _copyQueue = new();
    private readonly Queue<byte[]> _pipeQueue = new();
    private readonly Queue<byte[]> _freeBuffer = new();

    public FFmpegPipe(string ffmpegPath, string ffmpegArguments)
    {
        _ffmpegProcess = Process.Start(
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

        _ffmpegProcess.ErrorDataReceived += (sender, args) => OutputReceived?.Invoke(args.Data);
        _ffmpegProcess.BeginErrorReadLine();

        _copyThread = new Thread(CopyThread);
        _pipeThread = new Thread(PipeThread);
        _copyThread.Start();
        _pipeThread.Start();
    }

    public bool IsClosed
    {
        get => _threadsAreTerminated;
    }

    public void PushFrameData(NativeArray<byte> data)
    {
        lock (_copyQueue)
        {
            _copyQueue.Enqueue(data);
        }

        _copyStartEvent.Set();
    }

    public void SyncFrameData()
    {
        while (_copyQueue.Count > 0)
        {
            _copyEndEvent.WaitOne();
        }

        while (_pipeQueue.Count > 4)
        {
            _pipeEndEvent.WaitOne();
        }
    }

    public void Close()
    {
        if (_threadsAreTerminated)
        {
            return;
        }

        _threadsAreTerminated = true;

        _copyStartEvent.Set();
        _pipeStartEvent.Set();

        _copyThread.Join();
        _pipeThread.Join();

        _ffmpegProcess.StandardInput.Close();
        _ffmpegProcess.WaitForExit();

        _ffmpegProcess.Close();
        _ffmpegProcess.Dispose();
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
            _copyStartEvent.WaitOne();

            while (_copyQueue.Count > 0)
            {
                NativeArray<byte> source;
                lock (_copyQueue)
                {
                    source = _copyQueue.Peek();
                }

                byte[]? buffer = null;
                if (_freeBuffer.Count > 0)
                {
                    lock (_freeBuffer)
                    {
                        buffer = _freeBuffer.Dequeue();
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

                lock (_pipeQueue)
                {
                    _pipeQueue.Enqueue(buffer!);
                }

                _pipeStartEvent.Set();

                lock (_copyQueue)
                {
                    _copyQueue.Dequeue();
                }

                _copyEndEvent.Set();
            }
        }
    }

    private void PipeThread()
    {
        var ffmpegInputStream = _ffmpegProcess.StandardInput.BaseStream;

        while (_threadsAreTerminated is false)
        {
            _pipeStartEvent.WaitOne();

            while (_pipeQueue.Count > 0)
            {
                byte[] buffer;
                lock (_pipeQueue)
                {
                    buffer = _pipeQueue.Dequeue();
                }

                ffmpegInputStream.Write(buffer, 0, buffer.Length);
                ffmpegInputStream.Flush();

                lock (_freeBuffer)
                {
                    _freeBuffer.Enqueue(buffer);
                }

                _pipeEndEvent.Set();
            }
        }
    }
}
