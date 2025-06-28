using OuterScout.Shared.DependencyInjection;
using OuterScout.Shared.Extensions;
using OuterScout.Shared.Validation;
using OWML.Common;
using UnityEngine;

namespace OuterScout.Application.Recording;

using Application.FFmpeg;

public sealed partial class RenderTextureRecorder
{
    public sealed class Builder : IRecorder.IBuilder
    {
        private readonly string _targetFile;

        private readonly RenderTexture _texture;

        private int _constantRateFactor = 18;

        public Builder(string targetFile, RenderTexture renderTexture)
        {
            FFmpeg.ThrowIfNotAvailable();
            SystemInfo.supportsAsyncGPUReadback.Assert().IfFalse();

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
                    _ => throw new NotImplementedException(
                        $"{_texture.format.ToStringWithType()} input is not implemented"
                    ),
                },
            };

            var outputOptions = new FFmpegTextureEncoder.OutputOptions()
            {
                FilePath = _targetFile,
                FrameRate = Time.captureFramerate,
                PixelFormat = FFmpegPixelFormat.YUV420P,
                ConstantRateFactor = _constantRateFactor,
            };

            var textureEncoder = new FFmpegTextureEncoder(
                modConfig.GetFFmpegExecutablePathSetting(),
                inputOptions,
                outputOptions
            );

            SetupEncoderLogs(textureEncoder);

            return new RenderTextureRecorder(_texture, textureEncoder);
        }

        public Builder WithConstantRateFactor(int crf)
        {
            crf.Throw().IfLessThan(0).IfGreaterThan(63);
            _constantRateFactor = crf;
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
