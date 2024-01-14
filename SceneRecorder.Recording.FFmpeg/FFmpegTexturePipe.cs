using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Rendering;

// Modified from: https://github.com/keijiro/FFmpegOut

namespace SceneRecorder.Recording.FFmpeg;

internal sealed class FFmpegTexturePipe : IDisposable
{
    public event Action? RequestError;

    public event Action? TooManyRequests;

    private const int MaxReadbackRequests = 6;

    private FFmpegPipe? _Pipe;

    private readonly List<AsyncGPUReadbackRequest> _ReadbackQueue = new(4);

    public FFmpegTexturePipe(FFmpegPipe ffmpegPipe)
    {
        _Pipe = ffmpegPipe;
    }

    [MemberNotNullWhen(false, nameof(_Pipe))]
    public bool IsClosed
    {
        get => _Pipe is null;
    }

    public void PushFrame(Texture source)
    {
        if (IsClosed)
        {
            throw new InvalidOperationException($"{nameof(FFmpegTexturePipe)} is closed");
        }

        ProcessReadbackQueue();

        if (source != null)
        {
            QueueFrameReadback(source);
        }

        FlushFrames();
    }

    private void FlushFrames()
    {
        _Pipe?.SyncFrameData();
    }

    public void Close()
    {
        if (IsClosed)
        {
            return;
        }

        ProcessReadbackQueue();
        FlushFrames();
        _Pipe.Dispose();
        _Pipe = null;
    }

    public void Dispose()
    {
        Close();
    }

    private void QueueFrameReadback(Texture source)
    {
        if (_ReadbackQueue.Count > MaxReadbackRequests)
        {
            TooManyRequests?.Invoke();
            return;
        }

        var tempRenderTexture = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.ARGB32
        );

        Graphics.Blit(source, tempRenderTexture, new Vector2(1, -1), new Vector2(0, 1));

        _ReadbackQueue.Add(AsyncGPUReadback.Request(tempRenderTexture));

        RenderTexture.ReleaseTemporary(tempRenderTexture);
    }

    private void ProcessReadbackQueue()
    {
        while (_ReadbackQueue.Count > 0)
        {
            var firstRequest = _ReadbackQueue[0];

            if (firstRequest.done is false)
            {
                if (_ReadbackQueue.Count > 1 && _ReadbackQueue[1].done)
                {
                    firstRequest.WaitForCompletion();
                }
                else
                {
                    break;
                }
            }

            _ReadbackQueue.RemoveAt(0);

            if (firstRequest.hasError)
            {
                RequestError?.Invoke();
                continue;
            }

            _Pipe!.PushFrameData(firstRequest.GetData<byte>());
        }
    }
}
