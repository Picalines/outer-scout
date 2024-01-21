using OWML.Common;
using SceneRecorder.Recording.FFmpeg;
using SceneRecorder.Shared.DependencyInjection;
using SceneRecorder.Shared.Extensions;
using SceneRecorder.Shared.Validation;
using UnityEngine;

namespace SceneRecorder.Recording.Recorders;

internal sealed class RenderTextureRecorder : IRecorder
{
    public required RenderTexture Texture { get; init; }

    public required string TargetFile { get; init; }

    public required int FrameRate { get; init; }

    private FFmpegTextureEncoder? _ffmpegTextureEncoder = null;

    public void StartRecording()
    {
        Texture.ThrowIfNull();
        TargetFile.ThrowIfNull();
        FrameRate.ThrowIfNull().IfLessThan(1);

        var modConfig = Singleton<IModConfig>.Instance;
        var modConsole = Singleton<IModConsole>.Instance;

        var inputOptions = new FFmpegTextureEncoder.InputOptions()
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

        var outputOptions = new FFmpegTextureEncoder.OutputOptions()
        {
            FilePath = TargetFile,
            FrameRate = FrameRate,
            PixelFormat = FFmpegPixelFormat.YUV420P
        };

        _ffmpegTextureEncoder = new FFmpegTextureEncoder(
            Texture,
            modConfig.GetFFmpegExecutablePathSetting(),
            inputOptions,
            outputOptions
        );

        _ffmpegTextureEncoder.FFmpegOutputReceived += line =>
        {
            if (modConfig.GetEnableFFmpegLogsSetting())
            {
                modConsole.WriteLine($"ffmpeg: {line}", MessageType.Info);
            }
        };

        _ffmpegTextureEncoder.GpuReadbackError += () =>
            modConsole.WriteLine("Async GPU Readback error detected", MessageType.Error);

        _ffmpegTextureEncoder.TooManyGpuReadbackRequests += () =>
            modConsole.WriteLine("Too many Async GPU Readback requests", MessageType.Error);
    }

    public void RecordData()
    {
        _ffmpegTextureEncoder?.AddFrame(Texture);
    }

    public void StopRecording()
    {
        _ffmpegTextureEncoder!.Dispose();
        _ffmpegTextureEncoder = null;
    }
}
