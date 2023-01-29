using UnityEngine;
using UnityEngine.SceneManagement;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Extensions;

public static class LocatorExtensions
{
    public static bool IsInSolarSystemScene()
    {
        return SceneManager.GetActiveScene().name.Contains("SolarSystem");
    }

    public static GameObject? GetFreeCamera()
    {
        return GameObject.Find("FREECAM").Nullable();
    }

    public static GameObject? GetCurrentGroundBody()
    {
        return Locator.GetPlayerController().Nullable()?.GetLastGroundBody().Nullable()?.gameObject
            ?? Locator.GetAstroObject(AstroObject.Name.TimberHearth).Nullable()?.gameObject;
    }
}
