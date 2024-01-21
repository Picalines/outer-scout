using SceneRecorder.Recording.Domain;
using SceneRecorder.Shared.DependencyInjection;
using SceneRecorder.Shared.Extensions;
using SceneRecorder.Shared.Validation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SceneRecorder.Recording.Recorders;

public sealed class SceneRecorder : InitializedBehaviour<SceneRecorder.Parameters>
{
    public record struct Parameters
    {
        public required SceneSettings Settings { get; init; }
    }

    public bool IsRecording { get; private set; } = false;

    public int CurrentFrame { get; private set; }

    private readonly SceneSettings _settings;

    private readonly ComposedRecorder _recorders = new();

    private readonly UnityFrameNotifier _frameNotifier;

    private readonly ReversableAction[] _scenePatchers;

    private SceneRecorder()
        : base(out var parameters)
    {
        _settings = parameters.Settings;

        CurrentFrame = _settings.FrameRange.Start;

        _frameNotifier = gameObject.AddComponent<UnityFrameNotifier>();

        _scenePatchers = CreateScenePatchers();
    }

    private void OnDestroy()
    {
        IsRecording.Throw().IfTrue();

        Destroy(_frameNotifier);
    }

    public void AddRecorder(IRecorder recorder)
    {
        IsRecording.Throw().IfTrue();

        _recorders.AddRecorder(recorder);
    }

    public void StartRecording()
    {
        IsRecording.Throw().IfTrue();
        IsRecording = true;

        _scenePatchers.ForEach(action => action.Perform());

        _recorders.StartRecording();

        CurrentFrame = _settings.FrameRange.Start;

        _frameNotifier.FrameStarted += OnFrameStarted;
        _frameNotifier.FrameEnded += OnFrameEnded;
    }

    public int FramesRecorded
    {
        get => IsRecording ? CurrentFrame - _settings.FrameRange.Start : _settings.NumberOfFrames;
    }

    private void OnFrameStarted()
    {
        // TODO: apply keyframes
    }

    private void OnFrameEnded()
    {
        _recorders.RecordData();

        if (++CurrentFrame >= _settings.FrameRange.End)
        {
            _frameNotifier.FrameStarted -= OnFrameStarted;
            _frameNotifier.FrameEnded -= OnFrameEnded;

            _recorders.StopRecording();

            _scenePatchers.ForEach(action => action.Reverse());

            IsRecording = false;
        }
    }

    private ReversableAction[] CreateScenePatchers() =>
        [
            // time scale
            new(() =>
            {
                var timeScaleBeforeRecoding = Time.timeScale;
                Time.captureFramerate = _settings.FrameRate;

                return () =>
                {
                    Time.timeScale = timeScaleBeforeRecoding;
                    Time.captureFramerate = 0;
                };
            }),
            // input devices
            new(() =>
            {
                var inputDevicesToEnable = InputSystem
                    .devices.Where(device => device.enabled)
                    .ForEach(device => InputSystem.DisableDevice(device))
                    .ToArray();

                return () => inputDevicesToEnable.ForEach(InputSystem.EnableDevice);
            }),
            // pause menu
            new(() =>
            {
                var pauseMenuManager = Locator.GetPauseCommandListener()._pauseMenu;
                pauseMenuManager._pauseMenu.EnableMenu(false);

                return () => pauseMenuManager.TryOpenPauseMenu();
            }),
            // player invincibility
            new(() =>
            {
                var deathManager = Locator.GetDeathManager().OrNull();
                var playerResources = Locator
                    .GetPlayerTransform()
                    .OrNull()
                    ?.GetComponent<PlayerResources>();

                if (playerResources?.IsInvincible() is false)
                {
                    playerResources.ToggleInvincibility();
                }

                if (deathManager is { _invincible: false })
                {
                    deathManager.ToggleInvincibility();
                }

                return () =>
                {
                    playerResources?.ToggleInvincibility();
                    deathManager?.ToggleInvincibility();
                };
            }),
            // player model
            new(() =>
            {
                var playerRenderersToEnable = _settings.HidePlayerModel
                    ? Locator
                        .GetPlayerBody()
                        .OrNull()
                        ?.GetComponentsInChildren<Renderer>()
                        .Where(renderer => renderer.enabled)
                        .ForEach(renderer => renderer.enabled = false)
                        .ToArray()
                    : null;

                return () => playerRenderersToEnable?.ForEach(renderer => renderer.enabled = true);
            }),
            // quantum moon
            new(() =>
            {
                Locator.GetQuantumMoon().OrNull()?.SetActivation(false);

                return () => Locator.GetQuantumMoon().OrNull()?.SetActivation(true);
            })
        ];
}
