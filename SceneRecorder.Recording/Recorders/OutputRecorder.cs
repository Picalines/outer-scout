using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Recording.Animators;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders.Abstract;
using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.Shared.Models;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording.Recorders;

using TransformAnimator = Animators.TransformAnimator;

public sealed class OutputRecorder : RecorderComponent
{
    public IModConsole? ModConsole { get; set; } = null;

    public string? OutputDirectory { get; set; } = null;

    public IAnimator<TransformModel>? FreeCameraTransformAnimator { get; private set; } = null;

    public IAnimator<TransformModel>? HdriTransformAnimator { get; private set; } = null;

    private RecorderSettings? _Settings = null;

    private ComposedRecorder _ComposedRecorder = null!;

    private ComposedAnimator _ComposedAnimator = null!;

    private Action? _OnRecordingStarted = null;

    private Action? _OnRecordingFinished = null;

    private GameObject? _HdriPivot = null;

    public OutputRecorder()
    {
        Awoken += OnAwake;
    }

    private void OnAwake()
    {
        _ComposedRecorder = gameObject.AddComponent<ComposedRecorder>();

        RecordingStarted += OnRecordingStarted;
        RecordingFinished += OnRecordingFinished;
        BeforeFrameRecorded += OnBeforeFrameRecorded;
    }

    private void OnRecordingStarted()
    {
        _OnRecordingStarted?.Invoke();

        _ComposedRecorder.enabled = true;
    }

    private void OnRecordingFinished()
    {
        _ComposedRecorder.enabled = false;

        _OnRecordingFinished?.Invoke();
    }

    private void OnBeforeFrameRecorded()
    {
        _ComposedAnimator.SetFrame(FramesRecorded);

        if (FramesRecorded >= Settings!.FrameCount)
        {
            enabled = false;
        }
    }

    public RecorderSettings? Settings
    {
        get => _Settings;
        set
        {
            _Settings = value;
            Configure();
        }
    }

    [MemberNotNullWhen(true, nameof(Settings), nameof(OutputDirectory), nameof(ModConsole))]
    public bool IsAbleToRecord
    {
        get
        {
            return IsRecording
                || (Settings is not null
                && OutputDirectory is not null
                && ModConsole is not null
                && LocatorExtensions.IsInSolarSystemScene()
                && GameObject.Find("FREECAM").Nullable() is { } freeCamObject
                && freeCamObject.TryGetComponent<Camera>(out var freeCam)
                && freeCam.enabled);
        }
    }

    private void Configure()
    {
        if (IsRecording || !IsAbleToRecord)
        {
            throw new InvalidOperationException();
        }

        var player = Locator.GetPlayerBody();
        var freeCamera = GameObject.Find("FREECAM").GetComponent<OWCamera>();
        var groundBodyTransform = LocatorExtensions.GetCurrentGroundBody()!.transform;

        // background recorder
        var backgroundRecorder = freeCamera.gameObject.GetOrAddComponent<BackgroundRecorder>();
        (backgroundRecorder.Width, backgroundRecorder.Height) = Settings.Resolution;
        backgroundRecorder.FrameRate = Settings.FrameRate;

        // render background to GUI
        freeCamera.gameObject.GetOrAddComponent<RenderTextureRecorderGUI>();

        // depth recorder
        var depthRecorder = freeCamera.gameObject.GetOrAddComponent<DepthRecorder>();
        (depthRecorder.Width, depthRecorder.Height) = Settings.Resolution;
        depthRecorder.FrameRate = Settings.FrameRate;

        // hdri recorder
        _HdriPivot = _HdriPivot.Nullable();
        _HdriPivot ??= new GameObject($"{nameof(SceneRecorder)} HDRI Pivot");
        _HdriPivot.transform.parent = groundBodyTransform;

        var hdriRecorder = _HdriPivot.GetOrAddComponent<HDRIRecorder>();
        hdriRecorder.CubemapFaceSize = Settings.HDRIFaceSize;
        hdriRecorder.FrameRate = Settings.FrameRate;

        // combine recorders
        if (_ComposedRecorder.Recorders.Count == 0)
        {
            _ComposedRecorder.Recorders = new IRecorder[] { backgroundRecorder, depthRecorder, hdriRecorder };
        }

        foreach (var recorder in _ComposedRecorder.Recorders.OfType<RenderTextureRecorder>())
        {
            recorder.ModConsole = ModConsole;

            recorder.TargetFile = Path.Combine(OutputDirectory, recorder switch
            {
                BackgroundRecorder => "background.mp4",
                DepthRecorder => "depth.mp4",
                HDRIRecorder => "hdri.mp4",
                _ => throw new NotImplementedException(),
            });
        }

        // transform animators
        FreeCameraTransformAnimator = new TransformAnimator(freeCamera.transform);
        HdriTransformAnimator = new TransformAnimator(_HdriPivot.transform);

        _ComposedAnimator = new ComposedAnimator()
        {
            Animators = new[] { FreeCameraTransformAnimator, HdriTransformAnimator }!,
            FrameCount = Settings.FrameCount,
        };

        // start & end handlers
        var playerRenderersToToggle = Settings.HidePlayerModel
            ? player.gameObject.GetComponentsInChildren<Renderer>()
                .Where(renderer => renderer.enabled)
                .ToArray()
            : Array.Empty<Renderer>();

        _OnRecordingStarted = () =>
        {
            Array.ForEach(playerRenderersToToggle, renderer => renderer.enabled = false);
            Locator.GetQuantumMoon().SetActivation(false);

            freeCamera.transform.parent = groundBodyTransform;

            ModConsole.WriteLine($"Recording started ({OutputDirectory})");
        };

        _OnRecordingFinished = () =>
        {
            Array.ForEach(playerRenderersToToggle, renderer => renderer.enabled = true);
            Locator.GetQuantumMoon().SetActivation(true);

            ModConsole.WriteLine($"Recording finished ({OutputDirectory})");
        };
    }
}
