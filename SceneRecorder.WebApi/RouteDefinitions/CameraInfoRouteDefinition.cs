using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Shared.Models;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.RouteDefinitions;

using static ResponseFabric;

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

        serverBuilder.MapGet(
            ":entityName/camera_info",
            (string entityName) =>
            {
                if (entities.TryGetValue(entityName, out var entity) is false)
                {
                    return NotFound();
                }

                return Ok(CameraInfo.FromOWCamera(entity.GetOWCamera()));
            }
        );

        serverBuilder.MapPut(
            ":entityName/camera_info",
            (string entityName, CameraInfo cameraInfo) =>
            {
                if (entities.TryGetValue(entityName, out var entity) is false)
                {
                    return NotFound();
                }

                if (entity.Mutable is false)
                {
                    return NotAcceptable($"{entityName} camera info is immutable");
                }

                cameraInfo.ApplyToOWCamera(entity.GetOWCamera());

                return Ok();
            }
        );
    }
}
