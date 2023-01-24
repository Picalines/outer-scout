using UnityEngine.SceneManagement;

namespace Picalines.OuterWilds.SceneRecorder.WebApi;

internal static class LocatorExtensions
{
    public static bool IsInSolarSystemScene()
    {
        return SceneManager.GetActiveScene().name.Contains("SolarSystem");
    }
}
