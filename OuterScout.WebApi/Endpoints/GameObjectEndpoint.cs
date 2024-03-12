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

        public required TransformDTO Transform { get; init; }
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
        GameObjectRepository gameObjects
    )
    {
        if (_validApiGameObjectNameRegex.IsMatch(request.Name) is false)
        {
            return BadRequest($"invalid custom gameObject name");
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
            return BadRequest($"parent gameObject '{request.Transform.Parent}' not found");
        }

        var gameObject = new GameObject($"{nameof(OuterScout)} '{request.Name}'");
        gameObject.transform.ApplyWithParent(request.Transform.ToLocalTransform(parent));

        gameObjects.AddOwned(request.Name, gameObject);

        return Created();
    }

    private static IResponse GetGameObjectTransform(
        [FromUrl] string name,
        [FromUrl] string parent,
        GameObjectRepository gameObjects
    )
    {
        if (gameObjects.FindOrNull(name) is not { transform: var transform })
        {
            return BadRequest();
        }

        if (gameObjects.FindOrNull(parent) is not { transform: var parentTransform })
        {
            return BadRequest();
        }

        var globalTransform = new TransformDTO()
        {
            Position = transform.position,
            Rotation = transform.rotation,
            Scale = transform.lossyScale,
        };

        var localTransform = new TransformDTO()
        {
            Parent = parentTransform.name,
            Position = parentTransform.InverseTransformPoint(transform.position),
            Rotation = parentTransform.InverseTransformRotation(transform.rotation),
            Scale = transform.lossyScale,
        };

        return Ok(new { Global = globalTransform, Local = localTransform });
    }

    private static IResponse PutGameObjectTransform(
        [FromUrl] string name,
        [FromBody] TransformDTO transformDTO,
        GameObjectRepository gameObjects
    )
    {
        if (gameObjects.FindOrNull(name) is not { transform: var transform })
        {
            return BadRequest();
        }

        if (transform == Locator.GetPlayerCamera().OrNull()?.transform)
        {
            return MethodNotAllowed();
        }

        var parentTransform = transformDTO.Parent is { } parentName
            ? gameObjects.FindOrNull(parentName)?.transform
            : null;

        if ((transformDTO.Parent, parentTransform) is (not null, null))
        {
            return BadRequest();
        }

        if (parentTransform is null)
        {
            if (transformDTO.Position is { } globalPosition)
            {
                transform.position = globalPosition;
            }

            if (transformDTO.Rotation is { } globalRotation)
            {
                transform.rotation = globalRotation;
            }
        }
        else
        {
            if (transformDTO.Position is { } localPosition)
            {
                transform.position = parentTransform.TransformPoint(localPosition);
            }

            if (transformDTO.Rotation is { } localRotation)
            {
                transform.rotation = parentTransform.rotation * localRotation;
            }
        }

        if (transformDTO.Scale is { } localScale)
        {
            transform.localScale = localScale;
        }

        return Ok();
    }
}
