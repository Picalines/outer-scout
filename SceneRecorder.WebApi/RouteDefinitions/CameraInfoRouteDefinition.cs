using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Shared.Models;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.RouteDefinitions;

internal sealed class CameraInfoRouteDefinition : IApiRouteDefinition
{
    public static CameraInfoRouteDefinition Instance { get; } = new();

    private CameraInfoRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var precondition = serverBuilder.UseInPlayableScenePrecondition();

        var entities = new Dictionary<string, (bool Mutable, Func<OWCamera> GetOWCamera)>()
        {
            ["free_camera"] = (true, () => LocatorExtensions.GetFreeCamera()!),
            ["player_camera"] = (false, Locator.GetPlayerCamera),
        };

        serverBuilder.Map(
            HttpMethod.Get,
            ":entity_name/camera_info",
            (Request request, string entity_name) =>
            {
                if (entities.TryGetValue(entity_name, out var entity) is false)
                {
                    return ResponseFabric.NotFound();
                }

                return ResponseFabric.Ok(CameraInfo.FromOWCamera(entity.GetOWCamera()));
            }
        );

        serverBuilder.Map(
            HttpMethod.Put,
            ":entity_name/camera_info",
            (Request request, string entity_name) =>
            {
                if (entities.TryGetValue(entity_name, out var entity) is false)
                {
                    return ResponseFabric.NotFound();
                }

                if (entity.Mutable is false)
                {
                    return ResponseFabric.NotAcceptable($"{entity_name} camera info is immutable");
                }

                request.ParseContentJson<CameraInfo>().ApplyToOWCamera(entity.GetOWCamera());

                return ResponseFabric.Ok();
            }
        );
    }
}
