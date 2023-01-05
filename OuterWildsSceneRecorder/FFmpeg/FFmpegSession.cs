using OWML.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Rendering;

// Modified from: https://github.com/keijiro/FFmpegOut

namespace Picalines.OuterWilds.SceneRecorder.FFmpeg;

internal sealed class FFmpegSession : IDisposable
{
    private readonly IModConsole _ModConsole;

    private FFmpegPipe? _Pipe;

    private readonly List<AsyncGPUReadbackRequest> _ReadbackQueue = new(4);

    public static bool TryCreate(IModConsole modConsole, string arguments, [NotNullWhen(true)] out FFmpegSession? session)
    {
        if (SystemInfo.supportsAsyncGPUReadback is false)
        {
            modConsole.WriteLine("async gpu readback is not supported", MessageType.Error);
            session = null;
            return false;
        }

        session = new FFmpegSession(modConsole, arguments);
        return true;
    }

    private FFmpegSession(IModConsole modConsole, string arguments)
    {
        _ModConsole = modConsole;
        _Pipe = new FFmpegPipe(modConsole, arguments);
    }

    ~FFmpegSession()
    {
        if (_Pipe is not null)
        {
            _ModConsole.WriteLine("ffmpeg pipe closed before work finished", MessageType.Error);
        }
    }

    public void PushFrame(Texture source)
    {
        if (_Pipe is null)
        {
            return;
        }

        ProcessReadbackQueue();
        if (source != null)
        {
            QueueFrameReadback(source);
        }
    }

    public void CompletePushFrames()
    {
        _Pipe?.SyncFrameData();
    }

    public void Close()
    {
        if (_Pipe is null)
        {
            return;
        }

        _Pipe.Dispose();
        _Pipe = null;
    }

    public void Dispose()
    {
        Close();
    }

    private void QueueFrameReadback(Texture source)
    {
        if (_ReadbackQueue.Count > 6)
        {
            _ModConsole.WriteLine("Too many GPU readback requests", MessageType.Error);
            return;
        }

        var tempRenderTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
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
                _ModConsole.WriteLine("GPU readback error was detected", MessageType.Error);
                continue;
            }

            _Pipe!.PushFrameData(firstRequest.GetData<byte>());
        }
    }
}
