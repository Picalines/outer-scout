using SceneRecorder.Infrastructure.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SceneRecorder.Application.Recording;

public static class SceneRecorderExtensions
{
    public static SceneRecorder.Builder WithTimeScaleRestored(this SceneRecorder.Builder builder)
    {
        return builder.WithScenePatch(
            new(() =>
            {
                var timeScaleBeforeRecording = Time.timeScale;
                return () => Time.timeScale = timeScaleBeforeRecording;
            })
        );
    }

    public static SceneRecorder.Builder WithAllInputDevicesDisabled(
        this SceneRecorder.Builder builder
    )
    {
        return builder.WithScenePatch(
            new(() =>
            {
                var inputDevicesToEnable = InputSystem
                    .devices.Where(device => device.enabled)
                    .ForEach(device => InputSystem.DisableDevice(device))
                    .ToArray();

                return () => inputDevicesToEnable.ForEach(InputSystem.EnableDevice);
            })
        );
    }

    public static SceneRecorder.Builder WithPauseMenuDisabled(this SceneRecorder.Builder builder)
    {
        return builder.WithScenePatch(
            new(() =>
            {
                var pauseMenuManager = Locator.GetPauseCommandListener()._pauseMenu;
                pauseMenuManager._pauseMenu.EnableMenu(false);

                return () => pauseMenuManager.TryOpenPauseMenu();
            })
        );
    }

    public static SceneRecorder.Builder WithInvinciblePlayer(this SceneRecorder.Builder builder)
    {
        return builder.WithScenePatch(
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
            })
        );
    }

    public static SceneRecorder.Builder WithHiddenPlayerModel(this SceneRecorder.Builder builder)
    {
        return builder.WithScenePatch(
            new(() =>
            {
                var playerRenderersToEnable = Locator
                    .GetPlayerBody()
                    .OrNull()
                    ?.GetComponentsInChildren<Renderer>()
                    .Where(renderer => renderer.enabled)
                    .ForEach(renderer => renderer.enabled = false)
                    .ToArray();

                return () => playerRenderersToEnable?.ForEach(renderer => renderer.enabled = true);
            })
        );
    }

    public static SceneRecorder.Builder WithDisabledQuantumMoon(this SceneRecorder.Builder builder)
    {
        return builder.WithScenePatch(
            new(() =>
            {
                Locator.GetQuantumMoon().OrNull()?.SetActivation(false);

                return () => Locator.GetQuantumMoon().OrNull()?.SetActivation(true);
            })
        );
    }
}
