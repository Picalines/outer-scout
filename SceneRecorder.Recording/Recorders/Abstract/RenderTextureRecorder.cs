using OWML.Common;
using SceneRecorder.Recording.FFmpeg;
using UnityEngine;

namespace SceneRecorder.Recording.Recorders.Abstract;

internal abstract class RenderTextureRecorder : RecorderComponent
{
    public IModConfig? ModConfig { get; set; } = null;

    public IModConsole? ModConsole { get; set; } = null;

    public string? TargetFile { get; set; }

    public int FrameRate { get; set; }

    private RenderTexture? _SourceRenderTexture = null;

    private FFmpegTextureRecorder? _FFmpegRecorder = null;

    private bool _InitializedFrameEnded = false;

    public RenderTexture? SourceRenderTexture
    {
        get => _SourceRenderTexture;
    }

    protected abstract RenderTexture ProvideSourceRenderTexture();

    public RenderTextureRecorder()
    {
        RecordingStarted += OnRecordingStarted;

        RecordingFinished += OnRecordingFinished;
    }

    private void OnRecordingStarted()
    {
        _SourceRenderTexture ??= ProvideSourceRenderTexture();

        if (TargetFile is null)
            throw new ArgumentException(nameof(TargetFile));

        _FFmpegRecorder = new FFmpegTextureRecorder(
            ModConfig,
            ModConsole,
            _SourceRenderTexture,
            FrameRate,
            TargetFile
        );

        if (_InitializedFrameEnded is false)
        {
            FrameEnded += OnFrameEnded;
            _InitializedFrameEnded = true;
        }
    }

    private void OnFrameEnded()
    {
        _FFmpegRecorder!.RecordFrame();
    }

    private void OnRecordingFinished()
    {
        _FFmpegRecorder!.Dispose();
        _FFmpegRecorder = null;
    }
}
