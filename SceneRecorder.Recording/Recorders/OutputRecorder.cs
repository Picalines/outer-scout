using System.Diagnostics.CodeAnalysis;
using OWML.Common;
using SceneRecorder.Infrastructure.API;
using SceneRecorder.Recording.Recorders.Abstract;
using SceneRecorder.Shared.Extensions;
using SceneRecorder.Shared.Models;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SceneRecorder.Recording.Recorders;

using SceneRecorder.Recording.Animators;

public sealed class OutputRecorder : RecorderComponent
{
    public IModConfig? ModConfig { get; set; } = null;

    public IModConsole? ModConsole { get; set; } = null;

    public ICommonCameraAPI? CommonCameraAPI { get; set; } = null;

    public IAnimator<TransformDTO>? FreeCameraTransformAnimator { get; private set; } = null;
    public IAnimator<CameraDTO>? FreeCameraInfoAnimator { get; private set; } = null;
    public IAnimator<TransformDTO>? HdriTransformAnimator { get; private set; } = null;
    public IAnimator<float>? TimeScaleAnimator { get; private set; } = null;

    private RecorderSettingsDTO? _Settings = null;

    private ComposedAnimator? _ComposedAnimator = null;
    private ComposedRecorder _ComposedRecorder = null!;
    private IAnimator<CameraDTO>? _FreeCameraInfoAnimator = null;
    private IAnimator<CameraDTO>? _DepthCameraInfoAnimator = null;

    private GameObject? _HdriPivot = null;
    private OWCamera? _LastFreeCamera = null;

    private Action? _OnRecordingStarted = null;
    private Action? _OnRecordingFinished = null;
    private Action? _OnPauseMenuEnter = null;
    private Action? _OnPauseMenuExit = null;
    private IEnumerator<int>? _CurrentFrame = null;
    private bool _IsRecordingFinished = false;

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

        OWTime.OnPause += OnOWTimePause;
        OWTime.OnUnpause += OnOWTimeUnpause;
    }

    private void OnDestory()
    {
        OWTime.OnPause -= OnOWTimePause;
        OWTime.OnUnpause -= OnOWTimeUnpause;
    }

    private void OnRecordingStarted()
    {
        if (IsAbleToRecord is false)
        {
            throw new InvalidOperationException($"{nameof(IsAbleToRecord)} is false");
        }

        _CurrentFrame = _ComposedAnimator!.GetFrameNumbers().GetEnumerator();
        _CurrentFrame.MoveNext();

        _OnRecordingStarted?.Invoke();

        _ComposedRecorder.enabled = true;
        _IsRecordingFinished = false;

        ModConsole?.WriteLine($"Recording started ({Settings.OutputDirectory})", MessageType.Info);
    }

    private void OnRecordingFinished()
    {
        _CurrentFrame = null;

        _ComposedRecorder.enabled = false;

        _OnRecordingFinished?.Invoke();

        ModConsole?.WriteLine(
            $"Recording finished ({Settings!.OutputDirectory})",
            MessageType.Success
        );
    }

    private void OnFrameStarted()
    {
        _ComposedAnimator!.SetFrame(_CurrentFrame!.Current);
    }

    private void OnFrameEnded()
    {
        if (_IsRecordingFinished is true)
        {
            enabled = false;
            return;
        }

        if (_CurrentFrame?.MoveNext() is not true)
        {
            _IsRecordingFinished = true;
        }
    }

    public RecorderSettingsDTO? Settings
    {
        get => _Settings;
        set
        {
            if (IsRecording)
            {
                throw new InvalidOperationException(
                    $"cannot modify {nameof(Settings)} while recording"
                );
            }

            _Settings = value;
            Configure();
        }
    }

    [MemberNotNullWhen(true, nameof(Settings), nameof(ModConsole))]
    public bool IsAbleToRecord
    {
        get =>
            IsRecording
            || (
                Settings is not null
                && ModConsole is not null
                && CommonCameraAPI is not null
                && LocatorExtensions.IsInPlayableScene()
                && LocatorExtensions.GetFreeCamera() is not null
            );
    }

    private void Configure()
    {
        AssertPublicProperties();

        var playerBody = Locator.GetPlayerBody().OrThrow("player body not found");
        var playerCamera = Locator.GetPlayerCamera().OrThrow("player camera not found");
        var freeCamera = LocatorExtensions.GetFreeCamera().OrThrow("free camera not found");
        var groundBodyTransform = LocatorExtensions
            .GetCurrentGroundBody()
            .OrThrow("ground body transform not found")
            .transform;

        var enabledRecorders = new List<IRecorder>();

        var didSceneReload = freeCamera != _LastFreeCamera;
        _LastFreeCamera = freeCamera;

        if (didSceneReload)
        {
            FreeCameraTransformAnimator = null;
            FreeCameraInfoAnimator = null;
            HdriTransformAnimator = null;
            _FreeCameraInfoAnimator = null;
            _DepthCameraInfoAnimator = null;
        }

        // background recorder
        {
            var backgroundRecorder = freeCamera.GetOrAddComponent<BackgroundRecorder>();
            backgroundRecorder.TargetFile = Path.Combine(
                Settings.OutputDirectory,
                "background.mp4"
            );

            (backgroundRecorder.Width, backgroundRecorder.Height) = Settings.Resolution;
            backgroundRecorder.FrameRate = Settings.FrameRate;
            backgroundRecorder.ModConsole = ModConsole!;

            var progressGUI = freeCamera.GetOrAddComponent<RenderTextureRecorderGUI>();
            progressGUI.enabled = ModConfig?.GetEnableProgressUISetting() is true;

            enabledRecorders.Add(backgroundRecorder);
        }

        _FreeCameraInfoAnimator ??= new CameraInfoAnimator(freeCamera);
        var cameraAnimators = new List<IAnimator<CameraDTO>>() { _FreeCameraInfoAnimator };

        // depth recorder
        if (Settings.RecordDepth)
        {
            var depthRecorder = freeCamera.GetOrAddComponent<DepthRecorder>();
            depthRecorder.TargetFile = Path.Combine(Settings.OutputDirectory, "depth.mp4");
            (depthRecorder.Width, depthRecorder.Height) = Settings.Resolution;
            depthRecorder.FrameRate = Settings.FrameRate;
            depthRecorder.ModConsole = ModConsole!;

            _DepthCameraInfoAnimator ??= new CameraInfoAnimator(depthRecorder.DepthCamera);
            cameraAnimators.Add(_DepthCameraInfoAnimator);

            enabledRecorders.Add(depthRecorder);
        }

        // hdri recorder
        if (Settings.RecordHdri)
        {
            _HdriPivot =
                _HdriPivot.OrNull() ?? new GameObject($"{nameof(SceneRecorder)} HDRI Pivot");

            _HdriPivot.transform.parent = groundBodyTransform;

            var hdriRecorder = _HdriPivot.GetOrAddComponent<HdriRecorder>();
            hdriRecorder.TargetFile = Path.Combine(Settings.OutputDirectory, "hdri.mp4");
            hdriRecorder.CubemapFaceSize = Settings.HdriFaceSize;
            hdriRecorder.FrameRate = Settings.FrameRate;
            hdriRecorder.ModConsole = ModConsole!;

            enabledRecorders.Add(hdriRecorder);
        }

        // combine recorders
        if (didSceneReload)
        {
            _ComposedRecorder.Recorders = enabledRecorders.ToArray();
        }

        // animators
        {
            var animators = new List<IAnimator>
            {
                (FreeCameraTransformAnimator ??= new TransformAnimator(freeCamera.transform)),
                (
                    FreeCameraInfoAnimator = new ComposedAnimator<CameraDTO>()
                    {
                        Animators = cameraAnimators.ToArray(),
                    }
                ),
                (TimeScaleAnimator = Animators.TimeScaleAnimator.Instance),
            };

            if (_HdriPivot is not null)
            {
                animators.Add(
                    HdriTransformAnimator ??= new TransformAnimator(_HdriPivot.transform)
                );
            }

            _ComposedAnimator = new ComposedAnimator() { Animators = animators.ToArray() };

            _ComposedAnimator.SetFrameRange(Settings.StartFrame, Settings.EndFrame);
        }

        // start & end handlers

        var playerRenderersToToggle = Settings.HidePlayerModel
            ? playerBody
                .GetComponentsInChildren<Renderer>()
                .Where(renderer => renderer.enabled)
                .ToArray()
            : Array.Empty<Renderer>();

        var cameraDisplayEnablers = new[]
        {
            playerCamera.GetOrAddComponent<TargetDisplayEnabler>(),
            freeCamera.GetOrAddComponent<TargetDisplayEnabler>(),
        };

        float timeScaleBeforeRecording = 0;

        var pauseMenuManager = Locator.GetPauseCommandListener()._pauseMenu;
        var pauseMenu = pauseMenuManager._pauseMenu;

        var enabledInputDevices = Array.Empty<InputDevice>();

        var deathManager = Locator.GetDeathManager().OrNull();
        var playerResources = Locator
            .GetPlayerTransform()
            .OrNull()
            ?.GetComponent<PlayerResources>();

        _OnRecordingStarted = () =>
        {
            freeCamera.transform.parent = groundBodyTransform;
            CommonCameraAPI.EnterCamera(freeCamera);

            timeScaleBeforeRecording = Time.timeScale;
            Time.captureFramerate = Settings.FrameRate;

            DisableRenderingToDisplay();
            pauseMenu.EnableMenu(false);
            enabledInputDevices = InputSystem
                .devices.Where(device => device.enabled)
                .ForEach(device => InputSystem.DisableDevice(device))
                .ToArray();

            playerRenderersToToggle.ForEach(renderer => renderer.enabled = false);

            if (playerResources?.IsInvincible() is false)
            {
                playerResources.ToggleInvincibility();
            }

            if (deathManager?._invincible is false)
            {
                deathManager.ToggleInvincibility();
            }

            Locator.GetQuantumMoon().SetActivation(false);
        };

        _OnRecordingFinished = () =>
        {
            CommonCameraAPI.ExitCamera(freeCamera);

            Time.timeScale = timeScaleBeforeRecording;
            Time.captureFramerate = 0;

            EnableRenderingToDisplay();
            pauseMenuManager.TryOpenPauseMenu();
            enabledInputDevices.ForEach(InputSystem.EnableDevice);
            OWInput.ChangeInputMode(InputMode.All);

            playerRenderersToToggle.ForEach(renderer => renderer.enabled = true);

            playerResources?.ToggleInvincibility();
            deathManager?.ToggleInvincibility();

            Locator.GetQuantumMoon().OrNull()?.SetActivation(true);
        };

        if (ModConfig?.GetDisableRenderInPauseSetting() is true)
        {
            _OnPauseMenuEnter = DisableRenderingToDisplay;
            _OnPauseMenuExit = EnableRenderingToDisplay;
        }
        else
        {
            _OnPauseMenuEnter = null;
            _OnPauseMenuExit = null;
        }

        void EnableRenderingToDisplay()
        {
            cameraDisplayEnablers.ForEach(enabler => enabler.RenderingEnabled = true);
        }

        void DisableRenderingToDisplay()
        {
            cameraDisplayEnablers.ForEach(enabler =>
            {
                enabler.SaveDisplay();
                enabler.RenderingEnabled = false;
            });
        }
    }

    [MemberNotNull(nameof(Settings))]
    [MemberNotNull(nameof(CommonCameraAPI))]
    private void AssertPublicProperties()
    {
        if (Settings is null)
        {
            throw new ArgumentNullException($"{nameof(OutputRecorder)}.{nameof(Settings)} is null");
        }

        if (CommonCameraAPI is null)
        {
            throw new ArgumentNullException(
                $"{nameof(OutputRecorder)}.{nameof(CommonCameraAPI)} is null"
            );
        }
    }

    private void OnOWTimePause(OWTime.PauseType pauseType)
    {
        if (pauseType is OWTime.PauseType.Menu)
        {
            _OnPauseMenuEnter?.Invoke();
        }
    }

    private void OnOWTimeUnpause(OWTime.PauseType pauseType)
    {
        if (pauseType is OWTime.PauseType.Menu)
        {
            _OnPauseMenuExit?.Invoke();
        }
    }
}
