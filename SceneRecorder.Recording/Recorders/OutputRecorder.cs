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

    private IEnumerator<int>? _CurrentFrame = null;

    private bool _QueueEnd = false;

    public OutputRecorder()
    {
        Awoken += OnAwake;
    }

    private void OnAwake()
    {
        _ComposedRecorder = gameObject.AddComponent<ComposedRecorder>();

        RecordingStarted += OnRecordingStarted;
        RecordingFinished += OnRecordingFinished;
        FrameStarted += OnFrameStarted;
        FrameEnded += OnFrameEnded;
    }

    private void OnRecordingStarted()
    {
        if (IsAbleToRecord is false)
        {
            throw new InvalidOperationException($"{nameof(IsAbleToRecord)} is false");
        }

        _CurrentFrame = _ComposedAnimator.GetFrameNumbers().GetEnumerator();
        _CurrentFrame.MoveNext();

        _OnRecordingStarted?.Invoke();

        _ComposedRecorder.enabled = true;
        _QueueEnd = false;

        ModConsole?.WriteLine($"Recording started ({Settings.OutputDirectory})", MessageType.Info);
    }

    private void OnRecordingFinished()
    {
        _CurrentFrame = null;

        _ComposedRecorder.enabled = false;

        _OnRecordingFinished?.Invoke();

        ModConsole?.WriteLine($"Recording finished ({Settings!.OutputDirectory})", MessageType.Success);
    }

    private void OnFrameStarted()
    {
        _ComposedAnimator.SetFrame(_CurrentFrame!.Current);
    }

    private void OnFrameEnded()
    {
        if (_QueueEnd is true)
        {
            enabled = false;
            return;
        }

        if (_CurrentFrame?.MoveNext() is not true)
        {
            _QueueEnd = true;
        }
    }

    public RecorderSettings? Settings
    {
        get => _Settings;
        set
        {
            if (IsRecording)
            {
                throw new InvalidOperationException($"cannot modify {nameof(Settings)} while recording");
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

        var player = Locator.GetPlayerBody().NullIfDestroyed();
        var playerResources = Locator.GetPlayerTransform().NullIfDestroyed()?.GetComponent<PlayerResources>();
        var freeCamera = GameObject.Find("FREECAM").NullIfDestroyed()?.GetComponent<OWCamera>();
        var groundBodyTransform = LocatorExtensions.GetCurrentGroundBody()?.transform;
        var deathManager = Locator.GetDeathManager().NullIfDestroyed();

        if ((player, playerResources, freeCamera, groundBodyTransform, deathManager) is not ({ }, { }, { }, { }, { }))
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
        _HdriPivot = _HdriPivot.NullIfDestroyed();
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
                FreeCameraInfoAnimator = new ComposedAnimator<CameraInfo>()
                {
                    Animators = new IAnimator<CameraInfo>[]
                    {
                        new CameraInfoAnimator(freeCamera),
                        new CameraInfoAnimator(depthRecorder.DepthCamera),
                    },
                },
                HdriTransformAnimator = new TransformAnimator(_HdriPivot.transform),
                TimeScaleAnimator = Animators.TimeScaleAnimator.Instance,
            },
        };

        _ComposedAnimator.SetFrameRange(Settings.StartFrame, Settings.EndFrame);

        // start & end handlers
        var playerRenderersToToggle = Settings.HidePlayerModel
            ? player.gameObject.GetComponentsInChildren<Renderer>()
                .Where(renderer => renderer.enabled)
                .ToArray()
            : Array.Empty<Renderer>();

        float initialTimeScale = 0;
        var initialInputMode = InputMode.None;

        _OnRecordingStarted = () =>
        {
            Time.captureFramerate = Settings.FrameRate;

            CommonCameraAPI.EnterCamera(freeCamera);

            if (playerResources.IsInvincible() is false)
            {
                playerResources.ToggleInvincibility();
            }

            if (deathManager._invincible is false)
            {
                deathManager.ToggleInvincibility();
            }

            initialTimeScale = Time.timeScale;
            initialInputMode = OWInput.GetInputMode();

            Array.ForEach(playerRenderersToToggle, renderer => renderer.enabled = false);
            Locator.GetQuantumMoon().SetActivation(false);

            freeCamera.transform.parent = groundBodyTransform;

            OWInput.ChangeInputMode(InputMode.Menu);
        };

        _OnRecordingFinished = () =>
        {
            CommonCameraAPI.ExitCamera(freeCamera);

            playerResources.ToggleInvincibility();
            deathManager.ToggleInvincibility();

            Time.timeScale = initialTimeScale;
            Time.captureFramerate = 0;

            Array.ForEach(playerRenderersToToggle, renderer => renderer.enabled = true);
            Locator.GetQuantumMoon().SetActivation(true);

            OWInput.ChangeInputMode(initialInputMode);
        };
    }
}
