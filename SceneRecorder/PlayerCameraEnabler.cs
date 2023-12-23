using OWML.Common;
using SceneRecorder.Infrastructure.Extensions;
using UnityEngine;

namespace SceneRecorder;

internal sealed class PlayerCameraEnabler : MonoBehaviour
{
    private const int InvalidDisplay = -1;

    private int _TargetDisplay = 0;
    private bool _IsInPauseMenu = false;

    public void Configure(IModConfig config)
    {
        SetInitialState();

        if (config.GetSettingsValue<bool>("Disable rendering in pause"))
        {
            OWTime.OnPause += OnPause;
            OWTime.OnUnpause += OnUnpause;
        }
    }

    private void SetInitialState()
    {
        _IsInPauseMenu = false;
        ConfigurePlayerCamera();

        OWTime.OnPause -= OnPause;
        OWTime.OnUnpause -= OnUnpause;
    }

    private void OnDestroy()
    {
        SetInitialState();
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

    private void ConfigurePlayerCamera()
    {
        if (Locator.GetPlayerCamera().OrNull() is { } playerCamera)
        {
            var mainCamera = playerCamera.mainCamera;

            if (mainCamera.targetDisplay != InvalidDisplay)
            {
                _TargetDisplay = mainCamera.targetDisplay;
            }

            mainCamera.targetDisplay = !_IsInPauseMenu ? _TargetDisplay : InvalidDisplay;
        }
    }
}
