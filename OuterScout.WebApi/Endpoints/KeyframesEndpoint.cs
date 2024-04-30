using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using OuterScout.Application.Animation;
using OuterScout.Application.SceneCameras;
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

    private static readonly ConditionalWeakTable<
        PropertyAnimator,
        PropertyAnimatorData
    > _propertyAnimatorData = new();

    private KeyframesEndpoint() { }

    void IRouteMapper.MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithSceneCreatedFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            serverBuilder.MapPut("scene/keyframes", PutSceneKeyframes);

            serverBuilder.MapPut("objects/:name/keyframes", PutGameObjectKeyframes);
        }
    }

    public static IEnumerable<PropertyAnimator> GetOrderedPropertyAnimators(
        ApiResourceRepository apiResources
    )
    {
        // Properties are animated in a special order:
        // 1. scene properties (time.scale)
        // 2. scene.origin
        // 3. Outer Wilds objects (because they're animated relative to scene.origin)
        // 4. Outer Scout custom objects (because they might be parented to Outer Wilds objects)

        return apiResources
            .GetResources<PropertyAnimator>()
            .OrderBy(animator =>
                _propertyAnimatorData.TryGetValue(animator, out var data) ? data.Order : 0
            );
    }

    private sealed class KeyframeDto
    {
        public required float Value { get; init; }

        // TODO: other UnityEngine.Keyframe fields
    }

    private sealed class PropertyAnimationDto
    {
        public required Dictionary<int, KeyframeDto> Keyframes { get; init; }
    }

    private sealed class PutKeyframesRequest
    {
        public required Dictionary<string, PropertyAnimationDto> Properties { get; init; }
    }

    private sealed class PropertyAnimatorData
    {
        public required int Order { get; init; }
    }

    private static IResponse PutSceneKeyframes(
        [FromBody] PutKeyframesRequest request,
        ApiResourceRepository apiResources,
        GameObjectRepository gameObjects
    )
    {
        return PutKeyframes(apiResources, gameObjects, request, null);
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

        return PutKeyframes(apiResources, gameObjects, request, gameObject);
    }

    private static IResponse PutKeyframes(
        ApiResourceRepository apiResources,
        GameObjectRepository gameObjects,
        PutKeyframesRequest request,
        GameObject? gameObject
    )
    {
        var container = gameObject is not null
            ? apiResources.ContainerOf(gameObject)
            : apiResources.GlobalContainer;

        var keyframes = new Dictionary<AnimationCurve, PropertyAnimationDto>();

        foreach (var (property, animationDto) in request.Properties)
        {
            if (
                container.GetResource<PropertyAnimator>(property)
                is not { Curve: var propertyCurve }
            )
            {
                if (
                    CreatePropertyApplier(apiResources, gameObjects, gameObject, property)
                    is not { } propertyApplier
                )
                {
                    return BadRequest($"property '{property}' is not animatable");
                }

                propertyCurve = new AnimationCurve()
                {
                    preWrapMode = WrapMode.ClampForever,
                    postWrapMode = WrapMode.ClampForever
                };

                var propertyAnimator = new PropertyAnimator(propertyCurve, propertyApplier);
                container.AddResource(property, propertyAnimator);

                _propertyAnimatorData.Add(
                    propertyAnimator,
                    new() { Order = GetPropertyAnimatorOrder(gameObjects, gameObject) }
                );
            }

            keyframes.Add(propertyCurve, animationDto);
        }

        foreach (var (propertyCurve, animationDto) in keyframes)
        {
            foreach (var (frame, keyframeDto) in animationDto.Keyframes)
            {
                propertyCurve.AddKey(frame, keyframeDto.Value);
            }
        }

        return Ok();
    }

    private static int GetPropertyAnimatorOrder(
        GameObjectRepository gameObjects,
        GameObject? gameObject
    )
    {
        if (gameObject is null)
        {
            return 0;
        }

        if (ReferenceEquals(SceneEndpoint.GetOriginOrNull(gameObjects), gameObject.transform))
        {
            return 1;
        }

        return gameObjects.IsOwn(gameObject) ? 3 : 2;
    }

    private static PropertyApplier? CreatePropertyApplier(
        ApiResourceRepository apiResources,
        GameObjectRepository gameObjects,
        GameObject? gameObject,
        string property
    )
    {
        return gameObject switch
        {
            not null
                => TransformPropertyApplier(
                    GetOrAddTransformApplier(gameObjects, gameObject),
                    property
                ) ?? PerspectiveApplier(apiResources, gameObject, property),

            _ => ScenePropertyApplier(property)
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

    private static TransformApplier GetOrAddTransformApplier(
        GameObjectRepository gameObjects,
        GameObject gameObject
    )
    {
        var transformApplier = gameObject.GetOrAddComponent<TransformApplier>();

        transformApplier.Parent = gameObjects.IsOwn(gameObject)
            ? gameObject.transform.parent
            : SceneEndpoint.GetOriginOrNull(gameObjects).OrThrow();

        return transformApplier;
    }

    private static readonly Regex _transformPropertyRegex = new Regex(
        @"^transform\.(?<component>position|rotation|scale)\.(?<axis>[xyzw])$"
    );

    private static PropertyApplier? TransformPropertyApplier(
        TransformApplier transform,
        string property
    )
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
                    transform.LocalPosition = transform.LocalPosition.WithAxis(
                        axisIndex,
                        axisValue
                    ),

            ("rotation", >= 0 and <= 3)
                => axisValue =>
                    transform.LocalRotation = transform.LocalRotation.WithAxis(
                        axisIndex,
                        axisValue
                    ),

            ("scale", >= 0 and <= 2)
                => axisValue =>
                    transform.LocalScale = transform.LocalScale.WithAxis(axisIndex, axisValue),

            _ => null,
        };
    }

    private static readonly Regex _perspectivePropertyRegex = new Regex(
        @"^camera\.perspective\.(?<component>focalLength|sensorSize|lensShift|(?:near|far)ClipPlane)(?:\.(?<axis>[xy]))?$"
    );

    private static PropertyApplier? PerspectiveApplier(
        ApiResourceRepository apiResouces,
        GameObject gameObject,
        string property
    )
    {
        if (
            _perspectivePropertyRegex.Match(property) is not { Success: true, Groups: var groups }
            || apiResouces.ContainerOf(gameObject).GetResource<PerspectiveSceneCamera>()
                is not { } camera
        )
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

    // Unity normalizes the quaternion when assigning a localRotation, which makes
    // it impossible to consistently change each axis of the localRotation.
    // This class separately stores the unnormalized quaternion, and
    // applies it to the original transform.
    private sealed class TransformApplier : MonoBehaviour
    {
        private Transform _parent = null!;

        private Transform _transform = null!;

        private Vector3 _localPosition;
        private Quaternion _localRotation;
        private Vector3 _localScale;

        private void Awake()
        {
            _transform = transform;

            _localScale = _transform.localScale;
        }

        public Transform Parent
        {
            set
            {
                _parent = value;

                _localPosition = _parent.InverseTransformPoint(_transform.position);
                _localRotation = _parent.InverseTransformRotation(_transform.rotation);
            }
        }

        public Vector3 LocalPosition
        {
            get => _localPosition;
            set => _transform.position = _parent.TransformPoint(_localPosition = value);
        }

        public Quaternion LocalRotation
        {
            get => _localRotation;
            set => _transform.rotation = _parent.rotation * (_localRotation = value);
        }

        public Vector3 LocalScale
        {
            get => _localScale;
            set => _transform.localScale = _localScale = value;
        }
    }
}
