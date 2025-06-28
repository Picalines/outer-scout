using OuterScout.Application.Extensions;
using OuterScout.Shared.Extensions;
using OuterScout.WebApi.DTOs;
using OuterScout.WebApi.Extensions;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Services;
using UnityEngine;

namespace OuterScout.WebApi.Endpoints;

using static ResponseFabric;

internal sealed class GameObjectEndpoint : IRouteMapper
{
    public static GameObjectEndpoint Instance { get; } = new();

    private GameObjectEndpoint() { }

    void IRouteMapper.MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            using (serverBuilder.WithSceneCreatedFilter())
            {
                serverBuilder.MapPost("objects", PostCustomGameObject);
            }

            serverBuilder.MapGet("objects/:name", GetGameObject);

            serverBuilder.MapPut("objects/:name", PutGameObject);
        }
    }

    private sealed class PostCustomObjectRequest
    {
        public required string Name { get; init; }

        public TransformDto? Transform { get; init; }
    }

    private static IResponse PostCustomGameObject(
        [FromBody] PostCustomObjectRequest request,
        GameObjectRepository gameObjects
    )
    {
        if (request.Name is "" || request.Name.Contains("/") || request.Name.StartsWith("scene."))
        {
            return CommonResponse.InvalidBodyField(
                "name",
                "must be non-empty, cannot contain '/' and cannot start with the 'scene.'"
            );
        }

        if (gameObjects.FindOrNull(request.Name) is not null)
        {
            return CommonResponse.InvalidBodyField(
                "name",
                $"gameObject '{request.Name}' already exists"
            );
        }

        var (parent, parentName) = request.Transform switch
        {
            { Parent: { } transformDtoParent } => (
                gameObjects.FindOrNull(transformDtoParent)?.transform,
                transformDtoParent
            ),
            _ => (SceneEndpoint.GetOriginOrNull(gameObjects), SceneEndpoint.OriginResource),
        };

        if (parent is null)
        {
            return CommonResponse.GameObjectNotFound(parentName);
        }

        var gameObject = new GameObject($"{nameof(OuterScout)}#{request.Name}");
        gameObject.transform.parent = parent;
        gameObject.transform.ResetLocal();
        request.Transform?.ApplyLocal(gameObject.transform);

        gameObjects.AddOwned(request.Name, gameObject);

        return Created();
    }

    private static IResponse GetGameObject(
        [FromUrl] string name,
        GameObjectRepository gameObjects,
        [FromUrl] string origin = SceneEndpoint.OriginResource
    )
    {
        if (gameObjects.FindOrNull(name) is not { transform: var transform })
        {
            return CommonResponse.GameObjectNotFound(name);
        }

        if (gameObjects.FindOrNull(origin) is not { transform: var originTransform })
        {
            return CommonResponse.GameObjectNotFound(origin);
        }

        return Ok(
            new
            {
                Name = name,
                Transform = originTransform.InverseDto(transform) with
                {
                    Parent = transform.parent.OrNull()?.name,
                },
            }
        );
    }

    private sealed class PutGameObjectRequest
    {
        public TransformDto? Transform { get; init; }
    }

    private static IResponse PutGameObject(
        [FromUrl] string name,
        [FromBody] PutGameObjectRequest request,
        GameObjectRepository gameObjects,
        [FromUrl] string origin = SceneEndpoint.OriginResource
    )
    {
        if (gameObjects.FindOrNull(name) is not { transform: var transform })
        {
            return CommonResponse.GameObjectNotFound(name);
        }

        if (request is { Transform: { } transformDto })
        {
            if (transformDto.Parent is not null)
            {
                return BadRequest(
                    new Problem("parentCannotBeChanged")
                    {
                        Title = "Parent of the GameObject cannot be changed",
                    }
                );
            }

            if (gameObjects.FindOrNull(origin) is not { transform: var originTransform })
            {
                return CommonResponse.GameObjectNotFound(origin);
            }

            transformDto.ApplyGlobal(transform, originTransform);
        }

        return Ok();
    }
}
