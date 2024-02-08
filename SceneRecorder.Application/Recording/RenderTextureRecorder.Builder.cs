using OWML.Common;
using SceneRecorder.Application.FFmpeg;
using SceneRecorder.Infrastructure.DependencyInjection;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.Application.Recording;

public sealed partial class RenderTextureRecorder
{
    public sealed class Builder : IRecorder.IBuilder
    {
        private readonly string _targetFile;

        private readonly RenderTexture _texture;

        private int _frameRate = 30;

        public Builder(string targetFile, RenderTexture renderTexture)
        {
            _targetFile = targetFile;
            _texture = renderTexture;
        }

        public IRecorder StartRecording()
        {
            var modConfig = Singleton<IModConfig>.Instance;

            var inputOptions = new FFmpegTextureEncoder.InputOptions()
            {
                Width = _texture.width,
                Height = _texture.height,
                PixelFormat = _texture.format switch
                {
                    RenderTextureFormat.ARGB32 => FFmpegPixelFormat.RGBA,
                    _
                        => throw new NotImplementedException(
                            $"{_texture.format.ToStringWithType()} input is not supported"
                        ),
                },
            };

            var outputOptions = new FFmpegTextureEncoder.OutputOptions()
            {
                FilePath = _targetFile,
                FrameRate = _frameRate,
                PixelFormat = FFmpegPixelFormat.YUV420P
            };

            var textureEncoder = new FFmpegTextureEncoder(
                modConfig.GetFFmpegExecutablePathSetting(),
                inputOptions,
                outputOptions
            );

            SetupEncoderLogs(textureEncoder);

            return new RenderTextureRecorder(_texture, textureEncoder);
        }

        public Builder WithFrameRate(int frameRate)
        {
            frameRate.Throw().IfLessThan(1);
            _frameRate = frameRate;
            return this;
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
