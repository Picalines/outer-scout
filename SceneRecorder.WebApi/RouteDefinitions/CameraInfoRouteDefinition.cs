using SceneRecorder.Shared.Extensions;
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

        MapCameraRoutes(serverBuilder, "free-camera", true, LocatorExtensions.GetFreeCamera);

        MapCameraRoutes(serverBuilder, "player/camera", false, Locator.GetPlayerCamera);
    }

    private void MapCameraRoutes(
        HttpServerBuilder serverBuilder,
        string routePrefix,
        bool mutable,
        Func<OWCamera?> getOwCamera
    )
    {
        serverBuilder.MapGet(
            $"{routePrefix}/camera-info",
            () => getOwCamera() is { } camera ? Ok(CameraDTO.FromOWCamera(camera)) : NotFound()
        );

        if (mutable)
        {
            serverBuilder.MapPut(
                $"{routePrefix}/camera-info",
                (CameraDTO cameraInfo) =>
                {
                    if (getOwCamera() is not { } camera)
                    {
                        return NotFound();
                    }

                    cameraInfo.Apply(camera);

                    return Ok();
                }
            );
        }
    }
}
