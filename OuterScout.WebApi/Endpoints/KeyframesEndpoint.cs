using System.Text.RegularExpressions;
using OuterScout.Application.Animation;
using OuterScout.Application.SceneCameras;
using OuterScout.Domain;
using OuterScout.Infrastructure.Extensions;
using OuterScout.WebApi.Extensions;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Services;
using UnityEngine;

namespace OuterScout.WebApi.Endpoints;

using static ResponseFabric;

internal sealed class KeyframesEndpoint : IRouteMapper
{
    public static KeyframesEndpoint Instance { get; } = new();

    private KeyframesEndpoint() { }

    private sealed class KeyframeDTO
    {
        public required float Value { get; init; }
    }

    private class PutKeyframesRequest
    {
        public required string Property { get; init; }

        public required Dictionary<int, KeyframeDTO> Keyframes { get; init; }
    }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithSceneCreatedFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            serverBuilder.MapPut("scene/keyframes", PutSceneKeyframes);

            serverBuilder.MapPut("cameras/:id/keyframes", PutCameraKeyframes);

            serverBuilder.MapPut("gameObjects/:name/keyframes", PutGameObjectKeyframes);
        }
    }

    private static IResponse PutSceneKeyframes(
        [FromBody] PutKeyframesRequest request,
        ApiResourceRepository apiResources
    )
    {
        return PutKeyframes(request, apiResources.GlobalContainer, Unit.Instance);
    }

    private static IResponse PutCameraKeyframes(
        [FromUrl] string id,
        [FromBody] PutKeyframesRequest request,
        ApiResourceRepository apiResources
    )
    {
        if (
            apiResources.GlobalContainer.GetResource<ISceneCamera>(id)
            is not { Transform: { } transform } camera
        )
        {
            return CommonResponse.CameraNotFound(id);
        }

        return PutKeyframes(request, apiResources.ContainerOf(transform.gameObject), camera);
    }

    private static IResponse PutGameObjectKeyframes(
        [FromUrl] string name,
        [FromBody] PutKeyframesRequest request,
        ApiResourceRepository apiResources,
        GameObjectRepository gameObjects
    )
    {
        if (gameObjects.FindOrNull(name) is not { } gameObject)
        {
            return CommonResponse.GameObjectNotFound(name);
        }

        return PutKeyframes(request, apiResources.ContainerOf(gameObject), gameObject);
    }

    private static IResponse PutKeyframes<E>(
        PutKeyframesRequest request,
        IApiResourceContainer container,
        E entity
    )
    {
        if (
            container.GetResource<PropertyAnimator>(request.Property)
            is not { Curve: var propertyCurve } propertyAnimator
        )
        {
            if (CreatePropertyApplier(request.Property, entity) is not { } propertyApplier)
            {
                return BadRequest($"property '{request.Property}' is not animatable");
            }

            propertyCurve = new PropertyCurve();
            propertyAnimator = new PropertyAnimator(propertyCurve, propertyApplier);
            container.AddResource(request.Property, propertyAnimator);
        }

        foreach (var (frame, keyframeDto) in request.Keyframes)
        {
            propertyCurve.StoreKeyframe(new PropertyKeyframe(frame, keyframeDto.Value));
        }

        return Ok();
    }

    private static PropertyApplier? CreatePropertyApplier<E>(string property, E entity)
    {
        return entity switch
        {
            Unit => ScenePropertyApplier(property),

            GameObject { transform: var transform } => TransformApplier(property, transform),

            PerspectiveSceneCamera { Transform: var transform } perspectiveCamera
                => TransformApplier(property, transform)
                    ?? PerspectiveApplier(property, perspectiveCamera),

            ISceneCamera { Transform: var transform } => TransformApplier(property, transform),

            _ => null,
        };
    }

    private static PropertyApplier? ScenePropertyApplier(string property)
    {
        return property switch
        {
            "time.scale" => timeScale => Time.timeScale = timeScale,
            _ => null,
        };
    }

    private static readonly Regex _transformPropertyRegex = new Regex(
        @"^transform\.(?<component>position|rotation|scale)\.(?<axis>[xyzw])$"
    );

    private static PropertyApplier? TransformApplier(string property, Transform transform)
    {
        if (_transformPropertyRegex.Match(property) is not { Success: true, Groups: var groups })
        {
            return null;
        }

        var axisIndex = groups["axis"].Value switch
        {
            "x" => 0,
            "y" => 1,
            "z" => 2,
            "w" => 3,
            _ => -1
        };

        return (groups["component"].Value, axisIndex) switch
        {
            ("position", >= 0 and <= 2)
                => axisValue =>
                    transform.localPosition = transform.localPosition.WithAxis(
                        axisIndex,
                        axisValue
                    ),

            ("rotation", >= 0 and <= 3)
                => axisValue =>
                    transform.localRotation = transform.localRotation.WithAxis(
                        axisIndex,
                        axisValue
                    ),

            ("scale", >= 0 and <= 2)
                => axisValue =>
                    transform.localScale = transform.localScale.WithAxis(axisIndex, axisValue),

            _ => null,
        };
    }

    private static readonly Regex _perspectivePropertyRegex = new Regex(
        @"^perspective\.(?<component>focalLength|sensorSize|lensShift|(?:near|far)ClipPlane)(?:\.(?<axis>[xy]))?$"
    );

    private static PropertyApplier? PerspectiveApplier(
        string property,
        PerspectiveSceneCamera camera
    )
    {
        if (_perspectivePropertyRegex.Match(property) is not { Success: true, Groups: var groups })
        {
            return null;
        }

        const int noAxis = -1;

        var axisIndex = groups["axis"] switch
        {
            { Success: true, Value: "x" } => 0,
            { Success: true, Value: "y" } => 1,
            _ => noAxis
        };

        return (component: groups["component"].Value, axisIndex) switch
        {
            ("focalLength", noAxis)
                => length => camera.Perspective = camera.Perspective with { FocalLength = length },

            ("sensorSize", >= 0 and <= 1)
                => axisValue =>
                    camera.Perspective = camera.Perspective with
                    {
                        SensorSize = camera.Perspective.SensorSize.WithAxis(axisIndex, axisValue)
                    },

            ("lensShift", >= 0 and <= 1)
                => axisValue =>
                    camera.Perspective = camera.Perspective with
                    {
                        LensShift = camera.Perspective.LensShift.WithAxis(axisIndex, axisValue)
                    },

            ("nearClipPlane", noAxis)
                => plane => camera.Perspective = camera.Perspective with { NearClipPlane = plane },

            ("farClipPlane", noAxis)
                => plane => camera.Perspective = camera.Perspective with { FarClipPlane = plane },

            _ => null,
        };
    }
}
