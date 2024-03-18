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

    private sealed class CreateApiGameObjectRequest
    {
        public required string Name { get; init; }

        public required TransformDto Transform { get; init; }
    }

    private static readonly Regex _validApiGameObjectNameRegex = new Regex(
        @"^[a-zA-Z_][a-zA-Z0-9_\-]*$"
    );

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            using (serverBuilder.WithSceneCreatedFilter())
            {
                serverBuilder.MapPost("gameObjects", PostApiGameObject);
            }

            serverBuilder.MapGet("gameObjects/:name/transform", GetGameObjectTransform);

            serverBuilder.MapPut("gameObjects/:name/transform", PutGameObjectTransform);
        }
    }

    private static IResponse PostApiGameObject(
        [FromBody] CreateApiGameObjectRequest request,
        ApiResourceRepository resources,
        GameObjectRepository gameObjects
    )
    {
        if (_validApiGameObjectNameRegex.IsMatch(request.Name) is false)
        {
            return BadRequest("invalid custom gameObject name");
        }

        if (gameObjects.FindOrNull(request.Name) is not null)
        {
            return BadRequest($"gameObject '{request.Name}' already exists");
        }

        var parent = request.Transform.Parent is { } parentName
            ? gameObjects.FindOrNull(parentName)?.transform
            : null;

        if ((request.Transform.Parent, parent) is (not null, null))
        {
            return CommonResponse.GameObjectNotFound(request.Transform.Parent);
        }

        parent ??= resources
            .GlobalContainer.GetRequiredResource<GameObject>(SceneEndpoint.OriginResource)
            .transform;

        var gameObject = new GameObject($"{nameof(OuterScout)}#{request.Name}");
        gameObject.transform.parent = parent;
        gameObject.transform.ResetLocal();
        request.Transform.ApplyLocal(gameObject.transform);

        gameObjects.AddOwned(request.Name, gameObject);

        return Created();
    }

    private static IResponse GetGameObjectTransform(
        [FromUrl] string name,
        GameObjectRepository gameObjects,
        ApiResourceRepository resources,
        [FromUrl] string? origin = null
    )
    {
        if (gameObjects.FindOrNull(name) is not { transform: var transform })
        {
            return CommonResponse.GameObjectNotFound(name);
        }

        var originTransform = origin is not null ? gameObjects.FindOrNull(origin)?.transform : null;

        if ((origin, originTransform) is (not null, null))
        {
            return CommonResponse.GameObjectNotFound(origin);
        }

        originTransform ??= resources
            .GlobalContainer.GetResource<GameObject>(SceneEndpoint.OriginResource)
            ?.transform;

        if (originTransform is null)
        {
            return CommonResponse.SceneIsNotCreated;
        }

        return Ok(
            originTransform.InverseDto(transform) with
            {
                Parent = transform.parent.OrNull()?.name
            }
        );
    }

    private static IResponse PutGameObjectTransform(
        [FromUrl] string name,
        [FromBody] TransformDto transformDto,
        ApiResourceRepository resources,
        GameObjectRepository gameObjects
    )
    {
        if (gameObjects.FindOrNull(name) is not { transform: var transform })
        {
            return CommonResponse.GameObjectNotFound(name);
        }

        if (transform == Locator.GetPlayerCamera().OrNull()?.transform)
        {
            return MethodNotAllowed("can't modify player camera");
        }

        var parent = transformDto.Parent is { } parentName
            ? gameObjects.FindOrNull(parentName)?.transform
            : null;

        if ((transformDto.Parent, parent) is (not null, null))
        {
            return CommonResponse.GameObjectNotFound(transformDto.Parent);
        }

        parent ??= resources
            .GlobalContainer.GetResource<GameObject>(SceneEndpoint.OriginResource)
            ?.transform;

        if (parent is null)
        {
            return CommonResponse.SceneIsNotCreated;
        }

        transformDto.ApplyGlobal(transform, parent);

        return Ok();
    }
}
