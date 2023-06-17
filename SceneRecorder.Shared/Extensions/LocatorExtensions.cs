using UnityEngine;
using UnityEngine.SceneManagement;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Extensions;

public static class LocatorExtensions
{
    public static bool IsInPlayableScene()
    {
        return SceneManager.GetActiveScene().name is "SolarSystem" or "EyeOfTheUniverse";
    }

    public static GameObject? GetFreeCamera()
    {
        return GameObject.Find("FREECAM").OrNull();
    }

    public static GameObject? GetCurrentGroundBody()
    {
        return Locator.GetPlayerController().OrNull()?.GetLastGroundBody().OrNull()?.gameObject
            ?? Locator.GetAstroObject(AstroObject.Name.TimberHearth).OrNull()?.gameObject;
    }
}
