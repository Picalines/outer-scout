using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.WebApi.Extensions;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;
using Picalines.OuterWilds.SceneRecorder.Shared.Models;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class CameraInfoRouteDefinition : IApiRouteDefinition
{
    public static CameraInfoRouteDefinition Instance { get; } = new();

    private CameraInfoRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var precondition = serverBuilder.UseInGameScenePrecondition();

        var routeDefinitions = new Dictionary<string, (bool Mutable, Func<OWCamera> GetOWCamera)>()
        {
            ["free_camera"] = (true, () => LocatorExtensions.GetFreeCamera()!.GetAddComponent<OWCamera>()),
            ["player/camera"] = (false, Locator.GetPlayerCamera),
        };

        foreach (var (routePrefix, (mutable, getOWCamera)) in routeDefinitions)
        {
            serverBuilder.Map(HttpMethod.Get, $"{routePrefix}/camera_info", request =>
            {
                return ResponseFabric.Ok(CameraInfo.FromOWCamera(getOWCamera()));
            });

            if (mutable)
            {
                serverBuilder.Map(HttpMethod.Put, $"{routePrefix}/camera_info", request =>
                {
                    request.ParseContentJson<CameraInfo>()
                        .ApplyToOWCamera(getOWCamera());

                    return ResponseFabric.Ok();
                });
            }
        }
    }
}
