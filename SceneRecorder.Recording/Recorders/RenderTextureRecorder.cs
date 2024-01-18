using OWML.Common;
using SceneRecorder.Recording.FFmpeg;
using SceneRecorder.Shared.Extensions;
using SceneRecorder.Shared.Validation;
using UnityEngine;

namespace SceneRecorder.Recording.Recorders;

internal sealed class RenderTextureRecorder : IRecorder
{
    public required IModConfig ModConfig { get; init; }

    public required IModConsole ModConsole { get; init; }

    public required RenderTexture Texture { get; init; }

    public required string TargetFile { get; init; }

    public required int FrameRate { get; init; }

    private FFmpegTextureRecorder? _ffmpegTextureRecorder = null;

    public void StartRecording()
    {
        Texture.ThrowIfNull();
        TargetFile.ThrowIfNull();
        FrameRate.ThrowIfNull().IfLessThan(1);

        var inputOptions = new FFmpegTextureRecorder.InputOptions()
        {
            PixelFormat = Texture.format switch
            {
                RenderTextureFormat.ARGB32 => FFmpegPixelFormat.RGBA,
                RenderTextureFormat.RFloat => FFmpegPixelFormat.GrayF32LE,
                _
                    => throw new NotImplementedException(
                        $"{Texture.format.ToStringWithType()} input is not supported"
                    ),
            },
        };

        var outputOptions = new FFmpegTextureRecorder.OutputOptions()
        {
            FilePath = TargetFile,
            FrameRate = FrameRate,
            PixelFormat = FFmpegPixelFormat.YUV420P
        };

        _ffmpegTextureRecorder = new FFmpegTextureRecorder(
            Texture,
            ModConfig.GetFFmpegExecutablePathSetting(),
            inputOptions,
            outputOptions
        );

        _ffmpegTextureRecorder.FFmpegOutputReceived += line =>
        {
            if (ModConfig.GetEnableFFmpegLogsSetting())
            {
                ModConsole.WriteLine($"ffmpeg: {line}", MessageType.Info);
            }
        };

        _ffmpegTextureRecorder.GpuReadbackError += () =>
            ModConsole.WriteLine("Async GPU Readback error detected", MessageType.Error);

        _ffmpegTextureRecorder.TooManyGpuReadbackRequests += () =>
            ModConsole.WriteLine("Too many Async GPU Readback requests", MessageType.Error);
    }

    public void RecordData()
    {
        _ffmpegTextureRecorder?.RecordFrame();
    }

    public void StopRecording()
    {
        _ffmpegTextureRecorder!.Dispose();
        _ffmpegTextureRecorder = null;
    }
}
