using OWML.Common;
using SceneRecorder.Infrastructure.Extensions;
using UnityEngine;

namespace SceneRecorder;

internal sealed class PlayerCameraEnabler : MonoBehaviour
{
    private bool _IsInPauseMenu = false;

    public void Configure(IModConfig config)
    {
        OnUnpause(OWTime.PauseType.Menu);
        OnDestroy();

        if (config.GetSettingsValue<bool>("Disable rendering in pause"))
        {
            GlobalMessenger<OWCamera>.AddListener("SwitchActiveCamera", OnSwitchActiveCamera);

            OWTime.OnPause += OnPause;
            OWTime.OnUnpause += OnUnpause;
        }
    }

    private void OnDestroy()
    {
        GlobalMessenger<OWCamera>.RemoveListener("SwitchActiveCamera", OnSwitchActiveCamera);

        OWTime.OnPause -= OnPause;
        OWTime.OnUnpause -= OnUnpause;
    }

    private void OnPause(OWTime.PauseType pauseType)
    {
        if (pauseType is OWTime.PauseType.Menu)
        {
            _IsInPauseMenu = true;
            ConfigurePlayerCamera();
        }
    }

    private void OnUnpause(OWTime.PauseType pauseType)
    {
        if (pauseType is OWTime.PauseType.Menu)
        {
            _IsInPauseMenu = false;
            ConfigurePlayerCamera();
        }
    }

    private void OnSwitchActiveCamera(OWCamera camera)
    {
        this.InvokeAfterFrame(ConfigurePlayerCamera);
    }

    private void ConfigurePlayerCamera()
    {
        var playerCamera = Locator.GetPlayerCamera().OrNull();

        if (playerCamera is not null)
        {
            playerCamera.enabled = !_IsInPauseMenu;
        }
    }
}
