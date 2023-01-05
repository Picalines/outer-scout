using OWML.Common;
using System;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.FFmpeg;

internal sealed class FFmpegTextureRecorder : IDisposable
{
    public int FrameRate { get; }

    public string OutputFilePath { get; }

    public Texture SourceTexture { get; }

    public int FramesRendered { get; private set; } = 0;

    private readonly FFmpegSession _FFmpegSession;

    private bool _IsDisposed = false;

    public FFmpegTextureRecorder(IModConsole modConsole, Texture sourceTexture, int framerate, string outputFilePath)
    {
        SourceTexture = sourceTexture;
        FrameRate = framerate;
        OutputFilePath = outputFilePath;

        var ffmpegArguments = new CommandLineArguments()
            .Add("-y")
            .Add("-f rawvideo")
            .Add("-pix_fmt rgba")
            .Add("-colorspace bt709")
            .Add($"-video_size {sourceTexture.width}x{sourceTexture.height}")
            .Add($"-r {FrameRate}")
            .Add("-i -")
            .Add("-c:v libx264")
            .Add("-movflags +faststart")
            .Add("-pix_fmt yuv420p")
            .Add("-an")
            .Add("-crf 20")
            .Add("-q:v 0")
            .Add(OutputFilePath);

        FFmpegSession.TryCreate(modConsole, ffmpegArguments.ToString(), out _FFmpegSession!);
    }

    public void RenderFrame()
    {
        _FFmpegSession.PushFrame(SourceTexture);
        _FFmpegSession.CompletePushFrames();
        FramesRendered++;
    }

    public void Dispose()
    {
        if (_IsDisposed)
        {
            return;
        }

        _FFmpegSession.Dispose();

        _IsDisposed = true;
    }
}
