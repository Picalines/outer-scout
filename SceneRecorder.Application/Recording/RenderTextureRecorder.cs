using OWML.Common;
using SceneRecorder.Infrastructure.DependencyInjection;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Infrastructure.Validation;
using SceneRecorder.Application.FFmpeg;
using UnityEngine;

namespace SceneRecorder.Application.Recording;

public sealed class RenderTextureRecorder : IRecorder
{
    public sealed class Parameters
    {
        public required RenderTexture Texture { get; init; }

        public required string TargetFile { get; init; }

        public required int FrameRate { get; init; }
    }

    private readonly RenderTexture _texture;

    private FFmpegTextureEncoder? _ffmpegTextureEncoder = null;

    private RenderTexture? _coloredDepthTexture = null;

    private RenderTextureRecorder(RenderTexture texture, FFmpegTextureEncoder textureEncoder)
    {
        _texture = texture;

        _ffmpegTextureEncoder = textureEncoder;

        if (_texture.format is RenderTextureFormat.Depth)
        {
            _coloredDepthTexture = new RenderTexture(
                _texture.width,
                _texture.height,
                0,
                RenderTextureFormat.RFloat
            );
        }
    }

    public static RenderTextureRecorder StartRecording(Parameters parameters)
    {
        parameters.Texture.ThrowIfNull();
        parameters.TargetFile.ThrowIfNull();
        parameters.FrameRate.ThrowIfNull().IfLessThan(1);

        var texture = parameters.Texture;

        var modConfig = Singleton<IModConfig>.Instance;

        var inputOptions = new FFmpegTextureEncoder.InputOptions()
        {
            Width = texture.width,
            Height = texture.height,
            PixelFormat = texture.format switch
            {
                RenderTextureFormat.ARGB32 => FFmpegPixelFormat.RGBA,
                RenderTextureFormat.Depth => FFmpegPixelFormat.GrayF32LE,
                _
                    => throw new NotImplementedException(
                        $"{texture.format.ToStringWithType()} input is not supported"
                    ),
            },
        };

        var outputOptions = new FFmpegTextureEncoder.OutputOptions()
        {
            FilePath = parameters.TargetFile,
            FrameRate = parameters.FrameRate,
            PixelFormat = FFmpegPixelFormat.YUV420P
        };

        var textureEncoder = new FFmpegTextureEncoder(
            modConfig.GetFFmpegExecutablePathSetting(),
            inputOptions,
            outputOptions
        );

        SetupEncoderLogs(textureEncoder);

        return new RenderTextureRecorder(texture, textureEncoder);
    }

    public void Capture()
    {
        var textureToEncode = _texture;

        if (_coloredDepthTexture is not null)
        {
            Graphics.Blit(_texture, _coloredDepthTexture);
            textureToEncode = _coloredDepthTexture;
        }

        _ffmpegTextureEncoder?.AddFrame(textureToEncode);
    }

    public void Dispose()
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
