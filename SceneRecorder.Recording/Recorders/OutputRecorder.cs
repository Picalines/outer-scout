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
using SceneRecorder.Recording.FFmpeg;

public sealed class OutputRecorder : RecorderComponent
{
    public IModConsole? ModConsole { get; set; } = null;

    public ICommonCameraAPI? CommonCameraAPI { get; set; } = null;

    public IAnimator<TransformDTO>? FreeCameraTransformAnimator { get; private set; } = null;

    public IAnimator<CameraDTO>? FreeCameraInfoAnimator { get; private set; } = null;

    public IAnimator<TransformDTO>? HdriTransformAnimator { get; private set; } = null;

    public IAnimator<float>? TimeScaleAnimator { get; private set; } = null;

    private RecorderSettings? _Settings = null;

    private ComposedRecorder _ComposedRecorder = null!;

    private ComposedAnimator? _ComposedAnimator = null;

    private Action? _OnRecordingStarted = null;

    private Action? _OnRecordingFinished = null;

    private GameObject? _HdriPivot = null;

    private IEnumerator<int>? _CurrentFrame = null;

    private OWCamera? _LastFreeCamera = null;

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

        _CurrentFrame = _ComposedAnimator!.GetFrameNumbers().GetEnumerator();
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
                && FFmpeg.IsInstalled
            );
    }

    private void Configure()
    {
        if ((Settings, CommonCameraAPI) is not ({ }, { }))
        {
            throw new ArgumentNullException();
        }

        var player = Locator.GetPlayerBody().OrNull();
        var playerResources = Locator
            .GetPlayerTransform()
            .OrNull()
            ?.GetComponent<PlayerResources>();
        var freeCamera = LocatorExtensions.GetFreeCamera();
        var groundBodyTransform = LocatorExtensions.GetCurrentGroundBody()?.transform;
        var deathManager = Locator.GetDeathManager().OrNull();

        if (
            (player, playerResources, freeCamera, groundBodyTransform, deathManager)
            is not
            ({ }, { }, { }, { }, { })
        )
        {
            throw new InvalidOperationException("invalid scene state");
        }

        var didSceneReload = freeCamera != _LastFreeCamera;

        var addedRecorders = new List<IRecorder>();

        var cameraInfoAnimators = new List<IAnimator<CameraDTO>>()
        {
            new CameraInfoAnimator(freeCamera),
        };

        // background recorder
        {
            var backgroundRecorder = freeCamera.gameObject.GetOrAddComponent<BackgroundRecorder>();
            backgroundRecorder.TargetFile = Path.Combine(
                Settings.OutputDirectory,
                "background.mp4"
            );
            (backgroundRecorder.Width, backgroundRecorder.Height) = Settings.Resolution;
            backgroundRecorder.FrameRate = Settings.FrameRate;
            backgroundRecorder.ModConsole = ModConsole!;

            // render background to GUI
            var progressGUI = freeCamera.gameObject.GetOrAddComponent<RenderTextureRecorderGUI>();
            progressGUI.enabled = Settings.ShowProgressGUI;

            addedRecorders.Add(backgroundRecorder);
        }

        // depth recorder
        if (Settings.RecordDepth)
        {
            var depthRecorder = freeCamera.gameObject.GetOrAddComponent<DepthRecorder>();
            depthRecorder.TargetFile = Path.Combine(Settings.OutputDirectory, "depth.mp4");
            (depthRecorder.Width, depthRecorder.Height) = Settings.Resolution;
            depthRecorder.FrameRate = Settings.FrameRate;
            depthRecorder.ModConsole = ModConsole!;

            cameraInfoAnimators.Add(new CameraInfoAnimator(depthRecorder.DepthCamera));

            addedRecorders.Add(depthRecorder);
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

            addedRecorders.Add(hdriRecorder);
        }

        // combine recorders
        if (didSceneReload)
        {
            _ComposedRecorder.Recorders = addedRecorders.ToArray();
        }

        // animators
        {
            var animators = new List<IAnimator>
            {
                (FreeCameraTransformAnimator = new TransformAnimator(freeCamera.transform)),
                (
                    FreeCameraInfoAnimator = new ComposedAnimator<CameraDTO>()
                    {
                        Animators = cameraInfoAnimators.ToArray(),
                    }
                ),
                (TimeScaleAnimator = Animators.TimeScaleAnimator.Instance),
            };

            HdriTransformAnimator = null;
            if (_HdriPivot is not null)
            {
                animators.Add(HdriTransformAnimator = new TransformAnimator(_HdriPivot.transform));
            }

            if (didSceneReload)
            {
                _ComposedAnimator = null;
            }

            _ComposedAnimator ??= new ComposedAnimator() { Animators = animators.ToArray() };

            _ComposedAnimator.SetFrameRange(Settings.StartFrame, Settings.EndFrame);
        }

        // start & end handlers
        var playerRenderersToToggle = Settings.HidePlayerModel
            ? player
                .gameObject.GetComponentsInChildren<Renderer>()
                .Where(renderer => renderer.enabled)
                .ToArray()
            : Array.Empty<Renderer>();

        float initialTimeScale = 0;

        var pauseMenuManager = Locator.GetPauseCommandListener()._pauseMenu;
        var pauseMenu = pauseMenuManager._pauseMenu;

        var enabledInputDevices = Array.Empty<InputDevice>();

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

            Array.ForEach(playerRenderersToToggle, renderer => renderer.enabled = false);
            Locator.GetQuantumMoon().SetActivation(false);

            freeCamera.transform.parent = groundBodyTransform;

            enabledInputDevices = InputSystem.devices.Where(device => device.enabled).ToArray();
            Array.ForEach(enabledInputDevices, device => InputSystem.DisableDevice(device));

            pauseMenu.EnableMenu(false);
        };

        _OnRecordingFinished = () =>
        {
            CommonCameraAPI.ExitCamera(freeCamera);

            playerResources.ToggleInvincibility();
            deathManager.ToggleInvincibility();

            Time.timeScale = initialTimeScale;
            Time.captureFramerate = 0;

            Array.ForEach(playerRenderersToToggle, renderer => renderer.enabled = true);
            Locator.GetQuantumMoon().OrNull()?.SetActivation(true);

            Array.ForEach(enabledInputDevices, InputSystem.EnableDevice);
            pauseMenuManager.TryOpenPauseMenu();
            OWInput.ChangeInputMode(InputMode.All);
        };

        _LastFreeCamera = freeCamera;
    }
}
