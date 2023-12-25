using UnityEngine;

namespace SceneRecorder.Shared.Extensions;

public static class LocatorExtensions
{
    private static PlayerSpawner? _PlayerSpawner = null;

    public static bool IsInPlayableScene()
    {
        return LoadManager.GetCurrentScene() is OWScene.SolarSystem or OWScene.EyeOfTheUniverse;
    }

    public static OWCamera? GetFreeCamera()
    {
        return GameObject.Find("FREECAM").OrNull()?.GetComponent<OWCamera>();
    }

    public static GameObject? GetCurrentGroundBody()
    {
        return Locator.GetPlayerController().OrNull()?.GetLastGroundBody().OrNull()?.gameObject
            ?? Locator.GetAstroObject(AstroObject.Name.TimberHearth).OrNull()?.gameObject;
    }

    public static PlayerSpawner? GetPlayerSpawner()
    {
        if (_PlayerSpawner != null)
        {
            return _PlayerSpawner;
        }

        return _PlayerSpawner = GameObject
            .FindGameObjectWithTag("Player")
            .OrNull()
            ?.GetRequiredComponent<PlayerSpawner>();
    }
}
