using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.DTOs;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class TransformRouteMapper : IRouteMapper
{
    public static TransformRouteMapper Instance { get; } = new();

    private TransformRouteMapper() { }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            serverBuilder.MapGet("gameObjects/:name/transform", GetGameObjectTransform);

            serverBuilder.MapPut("gameObjects/:name/transform", PutGameObjectTransform);
        }
    }

    private static IResponse GetGameObjectTransform(string name, string parent)
    {
        if (GameObject.Find(name).OrNull() is not { transform: var transform })
        {
            return BadRequest();
        }

        if (GameObject.Find(parent).OrNull() is not { transform: var parentTransform })
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
            Rotation = transform.InverseTransformRotation(transform.rotation),
            Scale = transform.lossyScale,
        };

        return Ok(new { Global = globalTransform, Local = localTransform, });
    }

    private static IResponse PutGameObjectTransform(
        string name,
        [FromBody] TransformDTO transformDTO
    )
    {
        if (GameObject.Find(name).OrNull() is not { transform: var transform })
        {
            return BadRequest();
        }

        if (transform == Locator.GetPlayerCamera().OrNull()?.transform)
        {
            return MethodNotAllowed();
        }

        var parentTransform = transformDTO.Parent is { } parentName
            ? GameObject.Find(parentName).OrNull()?.transform
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
