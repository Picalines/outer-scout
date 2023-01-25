namespace Picalines.OuterWilds.SceneRecorder.WebApi;

internal static class PlayerControllerExtensions
{
    public static OWRigidbody GetLastGroundBodySafe(this PlayerCharacterController playerCharacterController)
    {
        var lastGroundBody = playerCharacterController.GetLastGroundBody();
        return lastGroundBody
            ?? Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetComponent<OWRigidbody>();
    }
}
