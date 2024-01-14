using OWML.Common;
using UnityEngine;

namespace SceneRecorder.Recording.FFmpeg;

public sealed class FFmpegTextureRecorder : IDisposable
{
    private readonly Texture _texture;

    private readonly FFmpegPipe _bytePipe;

    private readonly FFmpegTexturePipe _texturePipe;

    private bool _disposed = false;

    public FFmpegTextureRecorder(
        Texture texture,
        string ffmpegPath,
        string outputFilePath,
        int frameRate
    )
    {
        _texture = texture;

        _bytePipe = new FFmpegPipe(
            ffmpegPath,
            new CommandLineArguments()
                .Add("-y")
                .Add("-f rawvideo")
                .Add("-pix_fmt rgba")
                .Add("-colorspace bt709")
                .Add($"-video_size {texture.width}x{texture.height}")
                .Add($"-r {frameRate}")
                .Add("-i -")
                .Add("-an")
                .Add("-c:v libx265")
                .Add("-movflags +faststart")
                .Add("-crf 18")
                .Add("-q:v 0")
                .Add("-pix_fmt yuv420p")
                .Add($"\"{outputFilePath}\"")
                .ToString()
        );

        _texturePipe = new FFmpegTexturePipe(_bytePipe);
    }

    public required IModConsole? ModConsole
    {
        init
        {
            if (_disposed || value is null)
            {
                return;
            }

            _bytePipe.OutputReceived += line =>
                value.WriteLine($"FFmpeg: {line}", MessageType.Info);

            _texturePipe.RequestError += () =>
                value.WriteLine("Async GPU readback error detected", MessageType.Error);

            _texturePipe.TooManyRequests += () =>
                value.WriteLine("Too many async GPU readback requests", MessageType.Error);
        }
    }

    public void RecordFrame()
    {
        if (_disposed)
        {
            throw new InvalidOperationException($"{nameof(FFmpegPipe)} is disposed");
        }

        _texturePipe.PushFrame(_texture);
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
