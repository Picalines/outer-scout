using SceneRecorder.Shared.Extensions;
using SceneRecorder.Shared.Models;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteDefinitions;

using static ResponseFabric;

internal sealed record SetTransformRequest(TransformDTO Transform, string? LocalTo);

internal sealed class TransformRouteDefinition : IApiRouteDefinition
{
    public static TransformRouteDefinition Instance { get; } = new();

    private TransformRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var precondition = serverBuilder.UseInPlayableScenePrecondition();

        MapTransformRoutes(
            serverBuilder,
            "free-camera",
            true,
            () => LocatorExtensions.GetFreeCamera()?.transform
        );

        MapTransformRoutes(
            serverBuilder,
            "player/body",
            true,
            () => Locator.GetPlayerBody().OrNull()?.transform
        );

        MapTransformRoutes(
            serverBuilder,
            "player/camera",
            false,
            () => Locator.GetPlayerCamera().OrNull()?.transform
        );
    }

    private void MapTransformRoutes(
        HttpServerBuilder serverBuilder,
        string routePrefix,
        bool mutable,
        Func<Transform?> getTransform
    )
    {
        serverBuilder.MapGet(
            $"{routePrefix}/transform",
            (string localTo) =>
            {
                if (
                    getTransform() is not { } entityTransform
                    || GameObject.Find(localTo).OrNull() is not { transform: var origin }
                )
                {
                    return NotFound();
                }

                return Ok(
                    new
                    {
                        Origin = TransformDTO.FromGlobal(origin),
                        Transform = TransformDTO.FromInverse(origin, entityTransform)
                    }
                );
            }
        );

        if (mutable)
        {
            serverBuilder.MapPut(
                $"{routePrefix}/transform",
                (SetTransformRequest request) =>
                {
                    if (getTransform() is not { } entityTransform)
                    {
                        return NotFound();
                    }

                    var (newTransform, localTo) = request;

                    if (localTo is not null)
                    {
                        if (GameObject.Find(localTo).OrNull() is not { } origin)
                        {
                            return NotFound();
                        }

                        var oldEntityParent = entityTransform.parent;
                        entityTransform.parent = origin.transform;

                        newTransform.ApplyLocal(entityTransform);

                        entityTransform.parent = oldEntityParent;
                    }
                    else
                    {
                        newTransform.ApplyGlobal(entityTransform);
                    }

                    return Ok();
                }
            );
        }
    }
}
