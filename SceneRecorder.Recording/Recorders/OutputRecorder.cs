using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Recording.Animators;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders.Abstract;
using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.Shared.Interfaces;
using Picalines.OuterWilds.SceneRecorder.Shared.Models;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording.Recorders;

using TransformAnimator = Animators.TransformAnimator;

public sealed class OutputRecorder : RecorderComponent
{
    public IModConsole? ModConsole { get; set; } = null;

    public ICommonCameraAPI? CommonCameraAPI { get; set; } = null;

    public IAnimator<TransformModel>? FreeCameraTransformAnimator { get; private set; } = null;

    public IAnimator<CameraInfo>? FreeCameraInfoAnimator { get; private set; } = null;

    public IAnimator<TransformModel>? HdriTransformAnimator { get; private set; } = null;

    public IAnimator<float>? TimeScaleAnimator { get; private set; } = null;

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
        if (IsAbleToRecord is false)
        {
            throw new InvalidOperationException($"{nameof(IsAbleToRecord)} is false");
        }

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
        if (FramesRecorded >= Settings!.FrameCount)
        {
            enabled = false;
            return;
        }

        _ComposedAnimator.SetFrame(FramesRecorded);
    }

    public RecorderSettings? Settings
    {
        get => _Settings;
        set
        {
            if (IsRecording)
            {
                throw new InvalidOperationException();
            }

            _Settings = value;
            Configure();
        }
    }

    [MemberNotNullWhen(true, nameof(Settings), nameof(ModConsole))]
    public bool IsAbleToRecord
    {
        get
        {
            return IsRecording
                || (Settings is not null
                && ModConsole is not null
                && CommonCameraAPI is not null
                && LocatorExtensions.IsInSolarSystemScene()
                && GameObject.Find("FREECAM") != null);
        }
    }

    private void Configure()
    {
        if ((Settings, CommonCameraAPI) is not ({ }, { }))
        {
            throw new ArgumentNullException();
        }

        var player = Locator.GetPlayerBody().Nullable();
        var freeCamera = GameObject.Find("FREECAM").Nullable()?.GetComponent<OWCamera>();
        var groundBodyTransform = LocatorExtensions.GetCurrentGroundBody()?.transform;

        if ((player, freeCamera, groundBodyTransform) is not ({ }, { }, { }))
        {
            throw new InvalidOperationException();
        }

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

        var hdriRecorder = _HdriPivot.GetOrAddComponent<HdriRecorder>();
        hdriRecorder.CubemapFaceSize = Settings.HdriFaceSize;
        hdriRecorder.FrameRate = Settings.FrameRate;

        // combine recorders
        if (_ComposedRecorder.Recorders.Count == 0)
        {
            _ComposedRecorder.Recorders = new IRecorder[] { backgroundRecorder, depthRecorder, hdriRecorder };
        }

        foreach (var recorder in _ComposedRecorder.Recorders.OfType<RenderTextureRecorder>())
        {
            recorder.ModConsole = ModConsole!;

            recorder.TargetFile = Path.Combine(Settings.OutputDirectory, recorder switch
            {
                BackgroundRecorder => "background.mp4",
                DepthRecorder => "depth.mp4",
                HdriRecorder => "hdri.mp4",
                _ => throw new NotImplementedException(),
            });
        }

        // animators
        _ComposedAnimator = new ComposedAnimator()
        {
            Animators = new IAnimator[]
            {
                FreeCameraTransformAnimator = new TransformAnimator(freeCamera.transform),
                FreeCameraInfoAnimator = new CameraInfoAnimator(freeCamera),
                HdriTransformAnimator = new TransformAnimator(_HdriPivot.transform),
                TimeScaleAnimator = Animators.TimeScaleAnimator.Instance,
            }!,
            FrameCount = Settings.FrameCount,
        };

        // start & end handlers
        var playerRenderersToToggle = Settings.HidePlayerModel
            ? player.gameObject.GetComponentsInChildren<Renderer>()
                .Where(renderer => renderer.enabled)
                .ToArray()
            : Array.Empty<Renderer>();

        float initialTimeScale = 0;

        _OnRecordingStarted = () =>
        {
            CommonCameraAPI.EnterCamera(freeCamera);

            initialTimeScale = Time.timeScale;

            Array.ForEach(playerRenderersToToggle, renderer => renderer.enabled = false);
            Locator.GetQuantumMoon().SetActivation(false);

            freeCamera.transform.parent = groundBodyTransform;

            ModConsole?.WriteLine($"Recording started ({Settings.OutputDirectory})", MessageType.Info);
        };

        _OnRecordingFinished = () =>
        {
            CommonCameraAPI.ExitCamera(freeCamera);

            Time.timeScale = initialTimeScale;

            Array.ForEach(playerRenderersToToggle, renderer => renderer.enabled = true);
            Locator.GetQuantumMoon().SetActivation(true);

            ModConsole?.WriteLine($"Recording finished ({Settings.OutputDirectory})", MessageType.Success);
        };
    }
}
