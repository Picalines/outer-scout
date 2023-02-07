using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.Shared.Models;
using Picalines.OuterWilds.SceneRecorder.WebApi.Extensions;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class TransformRouteDefinition : IApiRouteDefinition
{
    public static TransformRouteDefinition Instance { get; } = new();

    private TransformRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var precondition = serverBuilder.UseInGameScenePrecondition();

        var routeDefinitions = new Dictionary<string, (bool Mutable, Func<Transform> GetTransform)>
        {
            ["free_camera"] = (true, () => LocatorExtensions.GetFreeCamera()!.transform),
            ["player/body"] = (true, () => Locator.GetPlayerBody().transform),
            ["player/camera"] = (false, () => Locator.GetPlayerCamera().transform),
        };

        foreach (var (routePrefix, (mutable, getTransform)) in routeDefinitions)
        {
            serverBuilder.MapGet($"{routePrefix}/transform/local_to/ground_body", request =>
            {
                var groundBodyTransform = LocatorExtensions.GetCurrentGroundBody()!.transform;
                var itemTransform = getTransform();

                return ResponseFabric.Ok(TransformModel.FromInverse(groundBodyTransform, itemTransform));
            });

            if (mutable)
            {
                serverBuilder.MapPut($"{routePrefix}/transform/local_to/ground_body", request =>
                {
                    var groundBodyTransform = LocatorExtensions.GetCurrentGroundBody()!.transform;
                    var itemTransform = getTransform();

                    var transformModel = request.ParseContentJson<TransformModel>();

                    var oldItemParent = itemTransform.parent;
                    itemTransform.parent = groundBodyTransform;
                    transformModel.ApplyToLocalTransform(itemTransform);
                    itemTransform.parent = oldItemParent;

                    return ResponseFabric.Ok();
                });
            }
        }
    }
}
