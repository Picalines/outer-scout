namespace Picalines.OuterWilds.SceneRecorder.WebApi;

internal static class PlayerControllerExtensions
{
    public static OWRigidbody GetLastGroundBodyOr(this PlayerCharacterController playerCharacterController, AstroObject.Name defaultAstroObject)
    {
        var lastGroundBody = playerCharacterController.GetLastGroundBody();
        return lastGroundBody
            ?? Locator.GetAstroObject(defaultAstroObject).GetComponent<OWRigidbody>();
    }
}
