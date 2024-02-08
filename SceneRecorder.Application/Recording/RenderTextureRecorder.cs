using SceneRecorder.Application.FFmpeg;
using UnityEngine;

namespace SceneRecorder.Application.Recording;

public sealed partial class RenderTextureRecorder : IRecorder
{
    private readonly RenderTexture _texture;

    private FFmpegTextureEncoder? _ffmpegTextureEncoder = null;

    private RenderTextureRecorder(RenderTexture texture, FFmpegTextureEncoder textureEncoder)
    {
        _texture = texture;
        _ffmpegTextureEncoder = textureEncoder;
    }

    public void Capture()
    {
        _ffmpegTextureEncoder?.AddFrame(_texture);
    }

    public void Dispose()
    {
        _ffmpegTextureEncoder!.Dispose();
        _ffmpegTextureEncoder = null;
    }
}
