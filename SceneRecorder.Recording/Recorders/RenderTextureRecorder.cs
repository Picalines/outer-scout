using OWML.Common;
using SceneRecorder.Recording.FFmpeg;
using SceneRecorder.Shared.DependencyInjection;
using SceneRecorder.Shared.Extensions;
using SceneRecorder.Shared.Validation;
using UnityEngine;

namespace SceneRecorder.Recording.Recorders;

public sealed class RenderTextureRecorder : IRecorder
{
    public required RenderTexture Texture { get; init; }

    public required string TargetFile { get; init; }

    public required int FrameRate { get; init; }

    private FFmpegTextureEncoder? _ffmpegTextureEncoder = null;

    private RenderTexture? _coloredDepthTexture = null;

    public void StartRecording()
    {
        Texture.ThrowIfNull();
        TargetFile.ThrowIfNull();
        FrameRate.ThrowIfNull().IfLessThan(1);

        var modConfig = Singleton<IModConfig>.Instance;

        var inputOptions = new FFmpegTextureEncoder.InputOptions()
        {
            Width = Texture.width,
            Height = Texture.height,
            PixelFormat = Texture.format switch
            {
                RenderTextureFormat.ARGB32 => FFmpegPixelFormat.RGBA,
                RenderTextureFormat.Depth => FFmpegPixelFormat.GrayF32LE,
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
            modConfig.GetFFmpegExecutablePathSetting(),
            inputOptions,
            outputOptions
        );

        SetupEncoderLogs(_ffmpegTextureEncoder);

        if (Texture.format is RenderTextureFormat.Depth)
        {
            _coloredDepthTexture = new RenderTexture(
                Texture.width,
                Texture.height,
                0,
                RenderTextureFormat.RFloat
            );
        }
    }

    public void RecordData()
    {
        var textureToEncode = Texture;

        if (_coloredDepthTexture is not null)
        {
            Graphics.Blit(Texture, _coloredDepthTexture);
            textureToEncode = _coloredDepthTexture;
        }

        _ffmpegTextureEncoder?.AddFrame(textureToEncode);
    }

    public void StopRecording()
    {
        _ffmpegTextureEncoder!.Dispose();
        _ffmpegTextureEncoder = null;

        if (_coloredDepthTexture is not null)
        {
            UnityEngine.Object.Destroy(_coloredDepthTexture);
            _coloredDepthTexture = null;
        }
    }

    private static void SetupEncoderLogs(FFmpegTextureEncoder encoder)
    {
        var modConfig = Singleton<IModConfig>.Instance;
        var modConsole = Singleton<IModConsole>.Instance;

        encoder.FFmpegOutputReceived += line =>
        {
            if (modConfig.GetEnableFFmpegLogsSetting())
            {
                modConsole.WriteLine($"ffmpeg: {line}", MessageType.Info);
            }
        };

        encoder.GpuReadbackError += () =>
            modConsole.WriteLine("Async GPU Readback error detected", MessageType.Error);

        encoder.TooManyGpuReadbackRequests += () =>
            modConsole.WriteLine("Too many Async GPU Readback requests", MessageType.Error);
    }
}
