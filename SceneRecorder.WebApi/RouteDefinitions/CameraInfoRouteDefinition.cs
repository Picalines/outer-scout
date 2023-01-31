using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;
using Picalines.OuterWilds.SceneRecorder.WebApi.Models;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class CameraInfoRouteDefinition : IApiRouteDefinition
{
    public static CameraInfoRouteDefinition Instance { get; } = new();

    private CameraInfoRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        var routeDefinitions = new Dictionary<string, (bool Mutable, Func<OWCamera> GetOWCamera)>()
        {
            ["free_camera"] = (true, () => LocatorExtensions.GetFreeCamera()!.GetAddComponent<OWCamera>()),
            ["player/camera"] = (false, Locator.GetPlayerCamera),
        };

        foreach (var (routePrefix, (mutable, getOWCamera)) in routeDefinitions)
        {
            serverBuilder.MapGet($"{routePrefix}/camera_info", request =>
            {
                if (LocatorExtensions.IsInSolarSystemScene() is false)
                {
                    return ResponseFabric.ServiceUnavailable();
                }

                return ResponseFabric.Ok(CameraInfo.FromOWCamera(getOWCamera()));
            });

            if (mutable)
            {
                serverBuilder.MapPut($"{routePrefix}/camera_info", request =>
                {
                    if (LocatorExtensions.IsInSolarSystemScene() is false)
                    {
                        return ResponseFabric.ServiceUnavailable();
                    }

                    request.ParseContentJson<CameraInfo>()
                        .ApplyToOWCamera(getOWCamera());

                    return ResponseFabric.Ok();
                });
            }
        }
    }
}
