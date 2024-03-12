using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Rendering;

// Modified from: https://github.com/keijiro/FFmpegOut

namespace OuterScout.Application.FFmpeg;

internal sealed class FFmpegTexturePipe : IDisposable
{
    public event Action? RequestError;

    public event Action? TooManyRequests;

    private const int MaxReadbackRequests = 6;

    private FFmpegPipe? _pipe;

    private readonly List<AsyncGPUReadbackRequest> _readbackQueue = new(4);

    public FFmpegTexturePipe(FFmpegPipe ffmpegPipe)
    {
        _pipe = ffmpegPipe;
    }

    [MemberNotNullWhen(false, nameof(_pipe))]
    public bool IsClosed
    {
        get => _pipe is null;
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
        _pipe?.SyncFrameData();
    }

    public void Close()
    {
        if (IsClosed)
        {
            return;
        }

        ProcessReadbackQueue();
        FlushFrames();
        _pipe.Dispose();
        _pipe = null;
    }

    public void Dispose()
    {
        Close();
    }

    private void QueueFrameReadback(Texture source)
    {
        if (_readbackQueue.Count > MaxReadbackRequests)
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

        _readbackQueue.Add(AsyncGPUReadback.Request(tempRenderTexture));

        RenderTexture.ReleaseTemporary(tempRenderTexture);
    }

    private void ProcessReadbackQueue()
    {
        while (_readbackQueue.Count > 0)
        {
            var firstRequest = _readbackQueue[0];

            if (firstRequest.done is false)
            {
                if (_readbackQueue.Count > 1 && _readbackQueue[1].done)
                {
                    firstRequest.WaitForCompletion();
                }
                else
                {
                    break;
                }
            }

            _readbackQueue.RemoveAt(0);

            if (firstRequest.hasError)
            {
                RequestError?.Invoke();
                continue;
            }

            _pipe!.PushFrameData(firstRequest.GetData<byte>());
        }
    }
}
