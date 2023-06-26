using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Extensions;

public static class LocatorExtensions
{
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
}
