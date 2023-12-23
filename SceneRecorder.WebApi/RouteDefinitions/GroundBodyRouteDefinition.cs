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
            "ground_body/name",
            () =>
                LocatorExtensions.GetCurrentGroundBody() is { } groundBody
                    ? Ok(groundBody.name)
                    : NotFound()
        );

        serverBuilder.MapGet(
            "ground_body/sectors/current/path",
            () =>
                Locator
                    .GetPlayerDetector()
                    .GetComponent<SectorDetector>()
                    .GetLastEnteredSector()
                    .transform.GetPath()
        );

        serverBuilder.MapPost(
            "ground_body/mesh_list",
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
