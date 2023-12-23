using SceneRecorder.BodyMeshExport;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.RouteDefinitions;

using static ResponseFabric;

internal sealed class GroundBodyRouteDefinition : IApiRouteDefinition
{
    public static GroundBodyRouteDefinition Instance { get; } = new();

    private GroundBodyRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var precondition = serverBuilder.UseInPlayableScenePrecondition();

        serverBuilder.MapGet(
            "player/ground-body/name",
            () =>
                LocatorExtensions.GetCurrentGroundBody() is { } groundBody
                    ? Ok(groundBody.name)
                    : NotFound()
        );

        serverBuilder.MapGet(
            "player/ground-body/sectors/current/path",
            () =>
                Locator
                    .GetPlayerDetector()
                    .GetComponent<SectorDetector>()
                    .GetLastEnteredSector()
                    .transform.GetPath()
        );

        serverBuilder.MapGet(
            "player/ground-body/meshes",
            () =>
                LocatorExtensions.GetCurrentGroundBody() switch
                {
                    { } groundBody
                        => Ok(GroundBodyMeshExport.CaptureMeshInfo(groundBody.gameObject)),
                    _ => NotFound(),
                }
        );
    }
}
