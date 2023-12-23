using SceneRecorder.Shared.Extensions;
using SceneRecorder.Shared.Models;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteDefinitions;

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

        serverBuilder.Map(
            HttpMethod.Get,
            ":entity_name/transform/local_to/ground_body",
            (Request request, string entity_name) =>
            {
                if (entities.TryGetValue(entity_name, out var entity) is false)
                {
                    return ResponseFabric.NotFound();
                }

                var groundBodyTransform = LocatorExtensions.GetCurrentGroundBody()!.transform;
                var entityTransform = entity.GetTransform();

                return ResponseFabric.Ok(
                    TransformModel.FromInverse(groundBodyTransform, entityTransform)
                );
            }
        );

        serverBuilder.Map(
            HttpMethod.Put,
            ":entity_name/transform/local_to/ground_body",
            (Request request, string entity_name) =>
            {
                if (entities.TryGetValue(entity_name, out var entity) is false)
                {
                    return ResponseFabric.NotFound();
                }

                if (entity.Mutable is false)
                {
                    return ResponseFabric.NotAcceptable($"{entity_name} transform is immutable");
                }

                var groundBodyTransform = LocatorExtensions.GetCurrentGroundBody()!.transform;
                var entityTransform = entity.GetTransform();

                var transformModel = request.ParseContentJson<TransformModel>();

                var oldItemParent = entityTransform.parent;
                entityTransform.parent = groundBodyTransform;
                transformModel.ApplyToLocalTransform(entityTransform);
                entityTransform.parent = oldItemParent;

                return ResponseFabric.Ok();
            }
        );
    }
}
