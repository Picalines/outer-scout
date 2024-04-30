using OuterScout.Infrastructure.Extensions;
using UnityEngine;

namespace OuterScout.Application.Extensions;

public static class LocatorExtensions
{
    private static PlayerSpawner? _playerSpawner = null;

    public static bool IsInPlayableScene()
    {
        return LoadManager.GetCurrentScene() is OWScene.SolarSystem or OWScene.EyeOfTheUniverse;
    }

    public static GameObject? GetLastGroundBody()
    {
        return Locator.GetPlayerController().OrNull()?.GetLastGroundBody().OrNull()?.gameObject;
    }

    public static PlayerSpawner? GetPlayerSpawner()
    {
        if (_playerSpawner != null)
        {
            return _playerSpawner;
        }

        return _playerSpawner = GameObject
            .FindGameObjectWithTag("Player")
            .OrNull()
            ?.GetRequiredComponent<PlayerSpawner>();
    }
}
