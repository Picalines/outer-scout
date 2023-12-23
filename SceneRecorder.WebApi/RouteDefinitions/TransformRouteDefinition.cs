using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Shared.Models;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteDefinitions;

using static ResponseFabric;

internal sealed class TransformRouteDefinition : IApiRouteDefinition
{
    public static TransformRouteDefinition Instance { get; } = new();

    private TransformRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var precondition = serverBuilder.UseInPlayableScenePrecondition();

        var entities = new Dictionary<string, (bool Mutable, Func<Transform> GetTransform)>
        {
            ["free_camera"] = (true, () => LocatorExtensions.GetFreeCamera()!.transform),
            ["player_body"] = (true, () => Locator.GetPlayerBody().transform),
            ["player_camera"] = (false, () => Locator.GetPlayerCamera().transform),
        };

        serverBuilder.MapGet(
            ":entityName/transform/local_to/ground_body",
            (string entityName) =>
            {
                if (entities.TryGetValue(entityName, out var entity) is false)
                {
                    return NotFound();
                }

                var groundBodyTransform = LocatorExtensions.GetCurrentGroundBody()!.transform;
                var entityTransform = entity.GetTransform();

                return Ok(TransformModel.FromInverse(groundBodyTransform, entityTransform));
            }
        );

        serverBuilder.MapPut(
            ":entityName/transform/local_to/ground_body",
            (string entityName, TransformModel newTransform) =>
            {
                if (entities.TryGetValue(entityName, out var entity) is false)
                {
                    return NotFound();
                }

                if (entity.Mutable is false)
                {
                    return NotAcceptable($"{entityName} transform is immutable");
                }

                var groundBodyTransform = LocatorExtensions.GetCurrentGroundBody()!.transform;
                var entityTransform = entity.GetTransform();

                var oldItemParent = entityTransform.parent;
                entityTransform.parent = groundBodyTransform;
                newTransform.ApplyToLocalTransform(entityTransform);
                entityTransform.parent = oldItemParent;

                return Ok();
            }
        );
    }
}
