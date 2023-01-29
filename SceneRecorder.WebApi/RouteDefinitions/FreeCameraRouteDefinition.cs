using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.Shared.Models;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;
using Picalines.OuterWilds.SceneRecorder.WebApi.Models;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class FreeCameraRouteDefinition : IApiRouteDefinition
{
    public static FreeCameraRouteDefinition Instance { get; } = new();

    private FreeCameraRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        serverBuilder.MapGet("free_camera/transform/local_to/ground_body", request =>
        {
            if (LocatorExtensions.IsInSolarSystemScene() is false)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            var groundBodyTransform = LocatorExtensions.GetCurrentGroundBody()!.transform;
            var freeCameraTransform = LocatorExtensions.GetFreeCamera()!.transform;

            return ResponseFabric.Ok(TransformModel.FromInverse(groundBodyTransform, freeCameraTransform));
        });

        serverBuilder.MapPut("free_camera/transform/local_to/ground_body", request =>
        {
            if (LocatorExtensions.IsInSolarSystemScene() is false)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            var groundBodyTransform = LocatorExtensions.GetCurrentGroundBody()!.transform;
            var freeCameraTransform = LocatorExtensions.GetFreeCamera()!.transform;

            var transformModel = request.ParseContentJson<TransformModel>();

            var oldCameraParent = freeCameraTransform.parent;
            freeCameraTransform.parent = groundBodyTransform;
            transformModel.ApplyToLocalTransform(freeCameraTransform);
            freeCameraTransform.parent = oldCameraParent;

            return ResponseFabric.Ok();
        });

        serverBuilder.MapGet("free_camera/info", request =>
        {
            if (LocatorExtensions.IsInSolarSystemScene() is false)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            var freeCam = LocatorExtensions.GetFreeCamera()!.GetComponent<OWCamera>();

            return ResponseFabric.Ok(CameraInfo.FromOWCamera(freeCam));
        });

        serverBuilder.MapPut("free_camera/info", request =>
        {
            if (LocatorExtensions.IsInSolarSystemScene() is false)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            var freeCam = LocatorExtensions.GetFreeCamera()!.GetComponent<OWCamera>();
            var newInfo = request.ParseContentJson<CameraInfo>();

            newInfo.ApplyToOWCamera(freeCam);

            return ResponseFabric.Ok();
        });
    }
}
