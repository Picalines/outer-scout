using UnityEngine;
using UnityEngine.SceneManagement;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Extensions;

public static class LocatorExtensions
{
    public static bool IsInPlayableScene()
    {
        return SceneManager.GetActiveScene().name is "SolarSystem" or "EyeOfTheUniverse";
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
