using OWML.Common;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording.FFmpeg;

public sealed class FFmpegTextureRecorder : IDisposable
{
    public int Framerate { get; }

    public string OutputFilePath { get; }

    public Texture SourceTexture { get; }

    public int FramesRecorded { get; private set; } = 0;

    private readonly FFmpegAsyncGPUReadback _FFmpegReadback;

    private bool _IsDisposed = false;

    public FFmpegTextureRecorder(
        IModConsole modConsole,
        Texture sourceTexture,
        int framerate,
        string outputFilePath
    )
    {
        SourceTexture = sourceTexture;
        Framerate = framerate;
        OutputFilePath = outputFilePath;

        var ffmpegArguments = new CommandLineArguments()
            .Add("-y")
            .Add("-f rawvideo")
            .Add("-pix_fmt rgba")
            .Add("-colorspace bt709")
            .Add($"-video_size {sourceTexture.width}x{sourceTexture.height}")
            .Add($"-r {Framerate}")
            .Add("-i -")
            .Add("-an")
            .Add("-c:v libx265")
            .Add("-movflags +faststart")
            .Add("-crf 18")
            .Add("-q:v 0")
            .Add("-pix_fmt yuv420p")
            .Add($"\"{OutputFilePath}\"");

        if (
            FFmpegAsyncGPUReadback.TryCreate(
                modConsole,
                ffmpegArguments.ToString(),
                out _FFmpegReadback!
            )
            is false
        )
        {
            throw new InvalidOperationException(
                $"failed to create {nameof(FFmpegAsyncGPUReadback)}"
            );
        }
    }

    public void RecordFrame()
    {
        if (_IsDisposed)
        {
            throw new InvalidOperationException($"{nameof(FFmpegPipe)} is disposed");
        }

        _FFmpegReadback.PushFrame(SourceTexture);
        _FFmpegReadback.CompletePushFrames();
        FramesRecorded++;
    }

    public void Dispose()
    {
        if (_IsDisposed)
        {
            return;
        }

        _FFmpegReadback.Dispose();

        _IsDisposed = true;
    }
}
