using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Recording.FFmpeg;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording.Recorders.Abstract;

internal abstract class RenderTextureRecorder : RecorderComponent
{
    public IModConsole ModConsole { get; set; } = null!;

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

        _FFmpegRecorder = new FFmpegTextureRecorder(ModConsole, _SourceRenderTexture, FrameRate, TargetFile);
        Time.captureFramerate = FrameRate;

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
        Time.captureFramerate = 0;

        _FFmpegRecorder!.Dispose();
        _FFmpegRecorder = null;
    }
}
