using OuterScout.Shared.Extensions;
using OWML.Common;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OuterScout.Application.Recording;

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

        return builder.WithScenePatch(
            () =>
            {
                activeCamera = Locator.GetActiveCamera().OrNull();
                if (activeCamera is not null)
                {
                    activeCamera.enabled = false;
                }
            },
            () =>
            {
                if (activeCamera is not null)
                {
                    activeCamera.enabled = true;
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
                    .Tap(device => InputSystem.DisableDevice(device))
                    .ToArray(),
            () => inputDevicesToEnable.ForEach(InputSystem.EnableDevice)
        );
    }

    public static SceneRecorder.Builder WithPauseMenuDisabled(this SceneRecorder.Builder builder)
    {
        var reopenPauseMenu = false;

        return builder.WithScenePatch(
            () =>
            {
                if (
                    Locator.GetPauseCommandListener() is
                    { _pauseMenu._pauseMenu: { _enabledMenu: true } pauseMenu }
                )
                {
                    pauseMenu.EnableMenu(false);
                    reopenPauseMenu = true;
                }
            },
            () =>
            {
                if (
                    reopenPauseMenu
                    && Locator.GetPauseCommandListener() is { _pauseMenu: { } pauseMenuManager }
                )
                {
                    pauseMenuManager.TryOpenPauseMenu();
                    reopenPauseMenu = false;
                }
            }
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
        Renderer[]? renderersToEnable = null;
        Light[]? lightsToEnable = null;

        return builder.WithScenePatch(
            () =>
            {
                var playerBody = Locator.GetPlayerBody().OrNull();

                renderersToEnable = playerBody
                    ?.GetComponentsInChildren<Renderer>()
                    .Where(renderer => renderer.enabled)
                    .Tap(renderer => renderer.enabled = false)
                    .ToArray();

                lightsToEnable = playerBody
                    ?.GetComponentsInChildren<Light>()
                    .Where(light => light.enabled)
                    .Tap(light => light.enabled = false)
                    .ToArray();
            },
            () =>
            {
                renderersToEnable?.ForEach(renderer => renderer.enabled = true);
                lightsToEnable?.ForEach(light => light.enabled = true);
            }
        );
    }

    public static SceneRecorder.Builder WithPlayerHeadVisible(this SceneRecorder.Builder builder)
    {
        GameObject? headMesh = null;
        GameObject? helmetMesh = null;

        int defaultLayer = LayerMask.NameToLayer("Default");
        int visibleToProbeLayer = LayerMask.NameToLayer("VisibleToProbe");

        return builder.WithScenePatch(
            () =>
            {
                headMesh = GameObject.Find(
                    "Player_Body/Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_Head"
                );

                helmetMesh = GameObject.Find(
                    "Player_Body/Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_Helmet"
                );

                if (GetHeadMeshBySuit() is { } meshToEnable)
                {
                    meshToEnable.layer = defaultLayer;
                }
            },
            () =>
            {
                if (GetHeadMeshBySuit() is { } meshToDisable)
                {
                    meshToDisable.layer = visibleToProbeLayer;
                }

                headMesh = helmetMesh = null;
            }
        );

        GameObject? GetHeadMeshBySuit()
        {
            return (Locator.GetPlayerSuit().IsWearingSuit() ? helmetMesh : headMesh).OrNull();
        }
    }

    public static SceneRecorder.Builder WithHudDisabled(this SceneRecorder.Builder builder)
    {
        GameObject? hudRoot = null;

        return builder.WithScenePatch(
            () =>
            {
                if (GUIMode.IsHiddenMode())
                {
                    return;
                }

                hudRoot = GameObject
                    .Find("Player_Body/PlayerCamera/Helmet/HelmetRoot/HelmetMesh")
                    .OrNull();
                hudRoot?.SetActive(false);
            },
            () =>
            {
                hudRoot?.SetActive(true);
                hudRoot = null;
            }
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
