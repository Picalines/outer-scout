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
        return GameObject.Find("FREECAM").NullIfDestroyed();
    }

    public static GameObject? GetCurrentGroundBody()
    {
        return Locator.GetPlayerController().NullIfDestroyed()?.GetLastGroundBody().NullIfDestroyed()?.gameObject
            ?? Locator.GetAstroObject(AstroObject.Name.TimberHearth).NullIfDestroyed()?.gameObject;
    }
}
