using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recorders;

internal abstract class RenderTextureRecorder : MonoBehaviour, IRecorder
{
    public string? TargetFile { get; set; }

    public int Framerate { get; set; }

    public bool RenderToGUI { get; set; } = false;

    public event Action? Awoken;

    public event Action? BeforeRecordingStarted;

    public event Action? BeforeFrameRecorded;

    public event Action? AfterRecordingFinished;

    private RenderTexture? _SourceRenderTexture = null;

    private FFMPEGVideoRenderer? _VideoRenderer = null;

    private bool _IsInAwake = false;

    private DateTime _StartedRecordingAt;

    private int _LastFramesRendered = 0;

    [MemberNotNullWhen(true, nameof(_VideoRenderer))]
    public bool IsRecording
    {
        get => _VideoRenderer is not null;
    }

    public int FramesRecorded
    {
        get => _VideoRenderer is not null
            ? _LastFramesRendered = _VideoRenderer.FramesRendered
            : _LastFramesRendered;
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

        _VideoRenderer = new FFMPEGVideoRenderer(_SourceRenderTexture, Framerate, TargetFile);
        _StartedRecordingAt = DateTime.Now;
        Time.captureFramerate = Framerate;
    }

    private void LateUpdate()
    {
        if (IsRecording is false)
        {
            return;
        }

        BeforeFrameRecorded?.Invoke();

        _VideoRenderer.RenderFrameAsync();
    }

    private void OnDisable()
    {
        if (_IsInAwake || IsRecording is false)
        {
            return;
        }

        Time.captureFramerate = 0;

        _ = FramesRecorded; // update counter

        _VideoRenderer.Dispose();
        _VideoRenderer = null;

        AfterRecordingFinished?.Invoke();
    }

    private void OnGUI()
    {
        if (this is not { RenderToGUI: true, IsRecording: true })
        {
            return;
        }

        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _SourceRenderTexture);

        TimeSpan elapsedRealtime = DateTime.Now - _StartedRecordingAt;
        TimeSpan elapsedVideo = TimeSpan.FromSeconds((float)_VideoRenderer.FramesRendered / Framerate);

        GUI.Box(new Rect(0, 0, 350, 80), GUIContent.none);
        GUI.Label(new Rect(10, 10, 500, 30), $"Rendered {elapsedVideo:hh':'mm':'ss} ({_VideoRenderer.FramesRendered} frames)");
        GUI.Label(new Rect(10, 40, 500, 30), $"Elapsed {elapsedRealtime:hh':'mm':'ss} ({elapsedRealtime.TotalSeconds / elapsedVideo.TotalSeconds:f3} times more)");
    }
}
