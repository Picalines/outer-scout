using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.FFmpeg;
using System;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recorders;

internal abstract class RenderTextureRecorder : RecorderComponent
{
    public IModConsole ModConsole { get; set; } = null!;

    public string? TargetFile { get; set; }

    public int FrameRate { get; set; }

    private RenderTexture? _SourceRenderTexture = null;

    private FFmpegTextureRecorder? _FFmpegRecorder = null;

    public RenderTexture? SourceRenderTexture
    {
        get => _SourceRenderTexture;
    }

    protected abstract RenderTexture ProvideSourceRenderTexture();

    public RenderTextureRecorder()
    {
        RecordingStarted += OnRecordingStarted;

        BeforeFrameRecorded += OnBeforeFrameRecorded;

        RecordingFinished += OnRecordingFinished;
    }

    private void OnRecordingStarted()
    {
        _SourceRenderTexture ??= ProvideSourceRenderTexture();

        if (TargetFile is null)
            throw new ArgumentException(nameof(TargetFile));

        _FFmpegRecorder = new FFmpegTextureRecorder(ModConsole, _SourceRenderTexture, FrameRate, TargetFile);
        Time.captureFramerate = FrameRate;
    }

    private void OnBeforeFrameRecorded()
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
