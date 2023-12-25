using SceneRecorder.BodyMeshExport;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Shared.Models;
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
            "player/ground-body",
            () =>
                LocatorExtensions.GetCurrentGroundBody() switch
                {
                    { name: var name, transform: var transform }
                        => Ok(
                            new
                            {
                                Name = name,
                                Transform = TransformDTO.FromGlobal(transform)
                            }
                        ),
                    _ => NotFound(),
                }
        );

        serverBuilder.MapGet(
            "player/ground-body/meshes",
            () =>
                LocatorExtensions.GetCurrentGroundBody() switch
                {
                    { } groundBody => Ok(GroundBodyMesh.GetDTO(groundBody)),
                    _ => NotFound(),
                }
        );
    }
}
