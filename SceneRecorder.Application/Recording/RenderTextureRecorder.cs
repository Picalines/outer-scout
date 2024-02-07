using SceneRecorder.Application.FFmpeg;
using UnityEngine;

namespace SceneRecorder.Application.Recording;

public sealed partial class RenderTextureRecorder : IRecorder
{
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
}
