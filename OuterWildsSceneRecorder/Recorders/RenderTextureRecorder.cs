using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.FFmpeg;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recorders;

internal abstract class RenderTextureRecorder : MonoBehaviour, IRecorder
{
    public IModConsole ModConsole { get; set; } = null!;

    public string? TargetFile { get; set; }

    public int Framerate { get; set; }

    public event Action? Awoken;

    public event Action? BeforeRecordingStarted;

    public event Action? BeforeFrameRecorded;

    public event Action? AfterRecordingFinished;

    private RenderTexture? _SourceRenderTexture = null;

    private FFmpegTextureRecorder? _FFmpegRecorder = null;

    private bool _IsInAwake = false;

    private int _LastFramesRendered = 0;

    [MemberNotNullWhen(true, nameof(_FFmpegRecorder))]
    public bool IsRecording
    {
        get => _FFmpegRecorder is not null;
    }

    public int FramesRecorded
    {
        get => _FFmpegRecorder is not null
            ? _LastFramesRendered = _FFmpegRecorder.FramesRecorded
            : _LastFramesRendered;
    }

    public RenderTexture? SourceRenderTexture
    {
        get => _SourceRenderTexture;
    }

    protected abstract RenderTexture ProvideSourceRenderTexture();

    private void Awake()
    {
        _IsInAwake = true;

        enabled = false;

        Awoken?.Invoke();
        _IsInAwake = false;
    }

    private void OnEnable()
    {
        _SourceRenderTexture ??= ProvideSourceRenderTexture();

        BeforeRecordingStarted?.Invoke();

        if (TargetFile is null)
            throw new ArgumentException(nameof(TargetFile));

        _FFmpegRecorder = new FFmpegTextureRecorder(ModConsole, _SourceRenderTexture, Framerate, TargetFile);
        Time.captureFramerate = Framerate;
    }

    private void LateUpdate()
    {
        if (IsRecording is false)
        {
            return;
        }

        BeforeFrameRecorded?.Invoke();

        _FFmpegRecorder.RecordFrame();
    }

    private void OnDisable()
    {
        if (_IsInAwake || IsRecording is false)
        {
            return;
        }

        Time.captureFramerate = 0;

        _ = FramesRecorded; // update counter

        _FFmpegRecorder.Dispose();
        _FFmpegRecorder = null;

        AfterRecordingFinished?.Invoke();
    }
}
