using OWML.Common;
using SceneRecorder.Infrastructure.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SceneRecorder.Application.Recording;

public static class SceneRecorderExtensions
{
    public static SceneRecorder.Builder WithProgressLoggedToConsole(
        this SceneRecorder.Builder builder,
        IModConsole modConsole
    )
    {
        return builder.WithScenePatch(
            () => modConsole.WriteLine("recording started", MessageType.Info),
            () => modConsole.WriteLine("recording finished", MessageType.Success)
        );
    }

    public static SceneRecorder.Builder WithTimeScaleRestored(this SceneRecorder.Builder builder)
    {
        float oldTimeScale = 1;

        return builder.WithScenePatch(
            () => oldTimeScale = Time.timeScale,
            () => Time.timeScale = oldTimeScale
        );
    }

    public static SceneRecorder.Builder WithDisplayRenderingDisabled(
        this SceneRecorder.Builder builder
    )
    {
        OWCamera? activeCamera = null;
        int targetDisplay = -1;

        return builder.WithScenePatch(
            () =>
            {
                activeCamera = Locator.GetActiveCamera().OrNull();
                if (activeCamera is not null)
                {
                    targetDisplay = activeCamera.mainCamera.targetDisplay;
                    activeCamera.mainCamera.targetDisplay = -1;
                }
            },
            () =>
            {
                if (activeCamera is not null)
                {
                    activeCamera.mainCamera.targetDisplay = targetDisplay;
                    activeCamera = null;
                }
            }
        );
    }

    public static SceneRecorder.Builder WithAllInputDevicesDisabled(
        this SceneRecorder.Builder builder
    )
    {
        var inputDevicesToEnable = Array.Empty<InputDevice>();

        return builder.WithScenePatch(
            () =>
                inputDevicesToEnable = InputSystem
                    .devices.Where(device => device.enabled)
                    .ForEach(device => InputSystem.DisableDevice(device))
                    .ToArray(),
            () => inputDevicesToEnable.ForEach(InputSystem.EnableDevice)
        );
    }

    public static SceneRecorder.Builder WithPauseMenuDisabled(this SceneRecorder.Builder builder)
    {
        return builder.WithScenePatch(
            () =>
                Locator.GetPauseCommandListener().OrNull()?._pauseMenu._pauseMenu.EnableMenu(false),
            () => Locator.GetPauseCommandListener().OrNull()?._pauseMenu.TryOpenPauseMenu()
        );
    }

    public static SceneRecorder.Builder WithInvinciblePlayer(this SceneRecorder.Builder builder)
    {
        return builder.WithScenePatch(
            () => SetPlayerInvincibility(true),
            () => SetPlayerInvincibility(false)
        );

        static void SetPlayerInvincibility(bool isInvincible)
        {
            if (
                Locator.GetDeathManager().OrNull() is { } deathManager
                && deathManager._invincible != isInvincible
            )
            {
                deathManager.ToggleInvincibility();
            }

            if (
                Locator.GetPlayerTransform().OrNull()?.GetComponent<PlayerResources>()
                    is { } playerResources
                && playerResources.IsInvincible() != isInvincible
            )
            {
                playerResources.ToggleInvincibility();
            }
        }
    }

    public static SceneRecorder.Builder WithHiddenPlayerModel(this SceneRecorder.Builder builder)
    {
        Renderer[]? playerRenderersToEnable = null;

        return builder.WithScenePatch(
            () =>
                playerRenderersToEnable = Locator
                    .GetPlayerBody()
                    .OrNull()
                    ?.GetComponentsInChildren<Renderer>()
                    .Where(renderer => renderer.enabled)
                    .ForEach(renderer => renderer.enabled = false)
                    .ToArray(),
            () => playerRenderersToEnable?.ForEach(renderer => renderer.enabled = true)
        );
    }

    public static SceneRecorder.Builder WithDisabledQuantumMoon(this SceneRecorder.Builder builder)
    {
        return builder.WithScenePatch(
            () => Locator.GetQuantumMoon().OrNull()?.SetActivation(false),
            () => Locator.GetQuantumMoon().OrNull()?.SetActivation(true)
        );
    }
}
