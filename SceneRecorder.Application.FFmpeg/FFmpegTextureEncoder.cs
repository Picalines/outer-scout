using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.Application.FFmpeg;

public sealed class FFmpegTextureEncoder : IDisposable
{
    public record struct InputOptions
    {
        public required int Width { get; init; }

        public required int Height { get; init; }

        public required FFmpegPixelFormat PixelFormat { get; init; }
    }

    public record struct OutputOptions
    {
        public required string FilePath { get; init; }

        public required int FrameRate { get; init; }

        public required FFmpegPixelFormat PixelFormat { get; init; }

        public required int ConstantRateFactor { get; init; }
    }

    public event Action<string>? FFmpegOutputReceived;

    public event Action? GpuReadbackError;

    public event Action? TooManyGpuReadbackRequests;

    private readonly FFmpegTexturePipe _texturePipe;

    private readonly int _inputWidth;

    private readonly int _inputHeight;

    private bool _disposed = false;

    public FFmpegTextureEncoder(
        string ffmpegPath,
        InputOptions inputOptions,
        OutputOptions outputOptions
    )
    {
        FFmpeg.ThrowIfNotAvailable();
        SystemInfo.supportsAsyncGPUReadback.Throw(e => new InvalidOperationException(e)).IfFalse();

        ffmpegPath.Throw().IfNullOrWhiteSpace();
        inputOptions.Width.Throw().IfLessThan(1);
        inputOptions.Height.Throw().IfLessThan(1);
        outputOptions.FrameRate.Throw().IfLessThan(1);
        outputOptions.ConstantRateFactor.Throw().IfLessThan(0).IfGreaterThan(63);
        outputOptions.FilePath.Throw().IfNullOrWhiteSpace();

        (_inputWidth, _inputHeight) = (inputOptions.Width, inputOptions.Height);

        var bytePipe = new FFmpegPipe(
            ffmpegPath,
            new CommandLineArguments()
                .Add("-y")
                .Add("-f rawvideo")
                .Add($"-pix_fmt {inputOptions.PixelFormat.ToCLIOption()}")
                .Add("-colorspace bt709")
                .Add($"-video_size {inputOptions.Width}x{inputOptions.Height}")
                .Add($"-r {outputOptions.FrameRate}")
                .Add("-i -")
                .Add("-an")
                .Add("-c:v libx265")
                .Add("-movflags +faststart")
                .Add($"-crf {outputOptions.ConstantRateFactor}")
                .Add("-q:v 0")
                .Add($"-pix_fmt {outputOptions.PixelFormat.ToCLIOption()}")
                .Add($"\"{outputOptions.FilePath}\"")
                .ToString()
        );

        _texturePipe = new FFmpegTexturePipe(bytePipe);

        bytePipe.OutputReceived += line => FFmpegOutputReceived?.Invoke(line);

        _texturePipe.RequestError += () => GpuReadbackError?.Invoke();

        _texturePipe.TooManyRequests += () => TooManyGpuReadbackRequests?.Invoke();
    }

    public void AddFrame(Texture texture)
    {
        if (_disposed)
        {
            throw new InvalidOperationException($"{nameof(FFmpegPipe)} is disposed");
        }

        texture.width.Throw().IfNotEquals(_inputWidth);
        texture.height.Throw().IfNotEquals(_inputHeight);

        _texturePipe.PushFrame(texture);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _texturePipe.Dispose();

        _disposed = true;
    }
}
