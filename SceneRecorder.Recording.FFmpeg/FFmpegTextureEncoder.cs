using SceneRecorder.Shared.Validation;
using UnityEngine;

namespace SceneRecorder.Recording.FFmpeg;

public sealed class FFmpegTextureEncoder : IDisposable
{
    public record struct InputOptions
    {
        public required FFmpegPixelFormat PixelFormat { get; init; }
    }

    public record struct OutputOptions
    {
        public required string FilePath { get; init; }

        public required int FrameRate { get; init; }

        public required FFmpegPixelFormat PixelFormat { get; init; }
    }

    public event Action<string>? FFmpegOutputReceived;

    public event Action? GpuReadbackError;

    public event Action? TooManyGpuReadbackRequests;

    private readonly FFmpegTexturePipe _texturePipe;

    private bool _disposed = false;

    public FFmpegTextureEncoder(
        Texture texture,
        string ffmpegPath,
        InputOptions inputOptions,
        OutputOptions outputOptions
    )
    {
        texture.Throw().IfNull();
        ffmpegPath.Throw().IfNullOrWhiteSpace();
        outputOptions.FrameRate.Throw().IfLessThan(1);
        outputOptions.FilePath.Throw().IfNullOrWhiteSpace();

        var bytePipe = new FFmpegPipe(
            ffmpegPath,
            new CommandLineArguments()
                .Add("-y")
                .Add("-f rawvideo")
                .Add($"-pix_fmt {inputOptions.PixelFormat.ToCLIOption()}")
                .Add("-colorspace bt709")
                .Add($"-video_size {texture.width}x{texture.height}")
                .Add($"-r {outputOptions.FrameRate}")
                .Add("-i -")
                .Add("-an")
                .Add("-c:v libx265")
                .Add("-movflags +faststart")
                .Add("-crf 18")
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
