using SceneRecorder.BodyMeshExport;
using SceneRecorder.Shared.Extensions;
using SceneRecorder.Shared.Models;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class GroundBodyRouteMapper : IRouteMapper
{
    public static GroundBodyRouteMapper Instance { get; } = new();

    private GroundBodyRouteMapper() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IRouteMapper.IContext context)
    {
        using var precondition = serverBuilder.UseInPlayableScenePrecondition();

        serverBuilder.MapGet(
            "player/ground-body",
            () =>
                LocatorExtensions.GetCurrentGroundBody() switch
                {
                    { name: var name, transform: var transform }
                        => Ok(
                            new GameObjectDTO
                            {
                                Name = name,
                                Path = transform.GetPath(),
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
                    { } groundBody => Ok(BodyMesh.GetDTO(groundBody)),
                    _ => NotFound(),
                }
        );
    }
}
