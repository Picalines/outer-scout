using System.Text.RegularExpressions;
using OuterScout.Application.Extensions;
using OuterScout.Infrastructure.Extensions;
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

    public void MapRoutes(HttpServer.Builder serverBuilder)
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

    private static readonly Regex _customObjectNameRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_\-]*$");

    private sealed class PostCustomObjectRequest
    {
        public required string Name { get; init; }

        public required TransformDto Transform { get; init; }
    }

    private static IResponse PostCustomGameObject(
        [FromBody] PostCustomObjectRequest request,
        GameObjectRepository gameObjects
    )
    {
        if (_customObjectNameRegex.IsMatch(request.Name) is false)
        {
            return BadRequest("invalid object name");
        }

        if (request.Transform.Parent is not { } transformDtoParent)
        {
            return BadRequest("gameObject must have a parent");
        }

        if (gameObjects.FindOrNull(request.Name) is not null)
        {
            return BadRequest($"gameObject '{request.Name}' already exists");
        }

        var transformDto = request.Transform;

        if (gameObjects.FindOrNull(transformDtoParent) is not { transform: var parent })
        {
            return CommonResponse.GameObjectNotFound(transformDtoParent);
        }

        var gameObject = new GameObject($"{nameof(OuterScout)}#{request.Name}");
        gameObject.transform.parent = parent;
        gameObject.transform.ResetLocal();
        transformDto.ApplyLocal(gameObject.transform);

        gameObjects.AddOwned(request.Name, gameObject);

        return Created();
    }

    private static IResponse GetGameObject(
        [FromUrl] string name,
        [FromUrl] string origin,
        GameObjectRepository gameObjects
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
                    Parent = transform.parent.OrNull()?.name
                }
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
        [FromUrl] string origin = ""
    )
    {
        if (gameObjects.FindOrNull(name) is not { transform: var transform })
        {
            return CommonResponse.GameObjectNotFound(name);
        }

        if (request is { Transform: { } transformDto })
        {
            if (origin is "")
            {
                return BadRequest(
                    new
                    {
                        Error = "origin parameter is required when changing the objects transform"
                    }
                );
            }

            if (transformDto.Parent is not null)
            {
                return BadRequest(new { Error = "parent gameObject cannot be changed" });
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
