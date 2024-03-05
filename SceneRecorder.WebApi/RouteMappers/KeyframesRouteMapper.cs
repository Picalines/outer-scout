using JsonSubTypes;
using Newtonsoft.Json;
using SceneRecorder.Application.Animation;
using SceneRecorder.Application.SceneCameras;
using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using SceneRecorder.WebApi.Services;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteMappers;

using SceneRecorder.Application.Recording;
using static JsonSubTypes.JsonSubtypes;
using static ResponseFabric;

internal sealed class KeyframesRouteMapper : IRouteMapper
{
    public static KeyframesRouteMapper Instance { get; } = new();

    private KeyframesRouteMapper() { }

    private sealed class KeyframeDTO<T>
    {
        public required T Value { get; init; }
    }

    [JsonConverter(typeof(JsonSubtypes), nameof(ISetKeyframesRequest.Property))]
    [KnownSubType(typeof(SetTimeScaleKeyframesRequest), "time.scale")]
    [KnownSubType(typeof(SetPositionKeyframesRequest), "transform.position")]
    [KnownSubType(typeof(SetRotationKeyframesRequest), "transform.rotation")]
    [KnownSubType(typeof(SetScaleKeyframesRequest), "transform.scale")]
    [KnownSubType(typeof(SetFocalLengthKeyframesRequest), "perspective.focalLength")]
    [KnownSubType(typeof(SetSensorSizeKeyframesRequest), "perspective.sensorSize")]
    [KnownSubType(typeof(SetLensShiftKeyframesRequest), "perspective.lensShift")]
    [KnownSubType(typeof(SetNearClipPlaneKeyframesRequest), "perspective.nearClipPlane")]
    [KnownSubType(typeof(SetFarClipPlaneKeyframesRequest), "perspective.farClipPlane")]
    private interface ISetKeyframesRequest
    {
        public string Property { get; }

        public IEnumerable<int> GetInvalidFrameNumbers(IntRange frameRange);

        public void StoreKeyframes(IAnimator animator);
    }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithSceneCreatedFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            serverBuilder.MapPut("gameObjects/:name/keyframes", PutGameObjectKeyframes);

            serverBuilder.MapPut("cameras/:id/keyframes", PutCameraKeyframes);

            serverBuilder.MapPut("scene/keyframes", PutSceneKeyframes);
        }
    }

    private static IResponse PutGameObjectKeyframes(
        [FromUrl] string name,
        [FromBody] ISetKeyframesRequest request,
        GameObjectRepository gameObjects,
        ApiResourceRepository apiResources
    )
    {
        if (
            apiResources.GlobalContainer.GetResource<SceneRecorder.Builder>()
            is not { } sceneRecorderBuilder
        )
        {
            return ServiceUnavailable();
        }

        if (gameObjects.FindOrNull(name) is not { } gameObject)
        {
            return NotFound();
        }

        var property = request.Property;
        var frameRange = sceneRecorderBuilder.FrameRange;
        var resources = apiResources.ContainerOf(gameObject);

        if (resources.GetResource<IAnimator>(property) is not { } animator)
        {
            animator = request switch
            {
                IAnimatorFactory<GameObject> f => f.CreateAnimator(gameObject, frameRange),
                IAnimatorFactory<Transform> f => f.CreateAnimator(gameObject.transform, frameRange),
                _ => null,
            };

            if (animator is null)
            {
                return BadRequest($"property '{property}' can't be animated");
            }

            resources.AddResource(property, animator);

            sceneRecorderBuilder.WithAnimator(animator);
        }

        if (request.GetInvalidFrameNumbers(frameRange).ToArray() is { Length: > 0 } invalidFrames)
        {
            return BadRequest(new { invalidFrames });
        }

        request.StoreKeyframes(animator);

        return Ok();
    }

    private static IResponse PutCameraKeyframes(
        [FromUrl] string id,
        [FromBody] ISetKeyframesRequest request,
        ApiResourceRepository apiResources
    )
    {
        if (
            apiResources.GlobalContainer.GetResource<SceneRecorder.Builder>()
            is not { } sceneRecorderBuilder
        )
        {
            return ServiceUnavailable();
        }

        if (
            apiResources.GlobalContainer.GetResource<ISceneCamera>(id)
            is not { Transform: { gameObject: var gameObject } transform } camera
        )
        {
            return NotFound($"camera '{id}' not found");
        }

        var property = request.Property;
        var frameRange = sceneRecorderBuilder.FrameRange;
        var resources = apiResources.ContainerOf(gameObject);

        if (resources.GetResource<IAnimator>(property) is not { } animator)
        {
            animator = request switch
            {
                IAnimatorFactory<PerspectiveSceneCamera> f
                    when camera is PerspectiveSceneCamera perspectiveCamera
                    => f.CreateAnimator(perspectiveCamera, frameRange),

                IAnimatorFactory<ISceneCamera> f => f.CreateAnimator(camera, frameRange),
                IAnimatorFactory<Transform> f => f.CreateAnimator(transform, frameRange),
                _ => null,
            };

            if (animator is null)
            {
                return BadRequest($"property '{property}' can't be animated");
            }

            resources.AddResource(property, animator);

            sceneRecorderBuilder.WithAnimator(animator);
        }

        if (request.GetInvalidFrameNumbers(frameRange).ToArray() is { Length: > 0 } invalidFrames)
        {
            return BadRequest(new { invalidFrames });
        }

        request.StoreKeyframes(animator);

        return Ok();
    }

    private static IResponse PutSceneKeyframes(
        [FromBody] ISetKeyframesRequest request,
        ApiResourceRepository apiResources
    )
    {
        var resources = apiResources.GlobalContainer;

        if (resources.GetResource<SceneRecorder.Builder>() is not { } sceneRecorderBuilder)
        {
            return ServiceUnavailable();
        }

        var property = request.Property;
        var frameRange = sceneRecorderBuilder.FrameRange;

        if (resources.GetResource<IAnimator>(property) is not { } animator)
        {
            animator = request switch
            {
                IAnimatorFactory f => f.CreateAnimator(frameRange),
                _ => null,
            };

            if (animator is null)
            {
                return BadRequest($"property '{property}' can't be animated");
            }

            resources.AddResource(property, animator);

            sceneRecorderBuilder.WithAnimator(animator);
        }

        if (request.GetInvalidFrameNumbers(frameRange).ToArray() is { Length: > 0 } invalidFrames)
        {
            return BadRequest(new { invalidFrames });
        }

        request.StoreKeyframes(animator);

        return Ok();
    }

    private interface IAnimatorFactory
    {
        public IAnimator CreateAnimator(IntRange frameRange);
    }

    private interface IAnimatorFactory<E>
    {
        public IAnimator CreateAnimator(E entity, IntRange frameRange);
    }

    private abstract class SetKeyframesRequest<T> : ISetKeyframesRequest
    {
        public abstract string Property { get; }

        [JsonIgnore]
        protected abstract ValueInterpolation<T> DefaultInterpolation { get; }

        public required IReadOnlyDictionary<int, KeyframeDTO<T>> Keyframes { get; init; }

        public IEnumerable<int> GetInvalidFrameNumbers(IntRange frameRange)
        {
            return Keyframes.Keys.Where(frame => frameRange.Contains(frame) is false);
        }

        public void StoreKeyframes(IAnimator animator)
        {
            if (animator is not Animator<T> { Keyframes: var keyframeStorage })
            {
                throw new NotImplementedException();
            }

            Keyframes
                .Select(p => new { Frame = p.Key, Dto = p.Value })
                .Select(f => new Keyframe<T>(f.Frame, f.Dto.Value, DefaultInterpolation))
                .ForEach(keyframeStorage.StoreKeyframe);
        }
    }

    private abstract class SetSceneKeyframesRequest<T> : SetKeyframesRequest<T>, IAnimatorFactory
    {
        protected abstract ValueApplier<T> Applier { get; }

        public IAnimator CreateAnimator(IntRange frameRange)
        {
            return new Animator<T>()
            {
                Keyframes = new KeyframeStorage<T>(frameRange),
                Applier = Applier,
            };
        }
    }

    private abstract class SetEntityKeyframesRequest<E, T>
        : SetKeyframesRequest<T>,
            IAnimatorFactory<E>
    {
        protected abstract ValueApplier<T> CreateApplier(E entity);

        public IAnimator CreateAnimator(E entity, IntRange frameRange)
        {
            return new Animator<T>()
            {
                Keyframes = new KeyframeStorage<T>(frameRange),
                Applier = CreateApplier(entity),
            };
        }
    }

    private sealed class SetTimeScaleKeyframesRequest : SetSceneKeyframesRequest<float>
    {
        public override string Property { get; } = "time.scale";

        protected override ValueApplier<float> Applier { get; } = s => Time.timeScale = s;

        protected override ValueInterpolation<float> DefaultInterpolation { get; } = Mathf.Lerp;
    }

    private sealed class SetPositionKeyframesRequest : SetEntityKeyframesRequest<Transform, Vector3>
    {
        public override string Property { get; } = "transform.position";

        protected override ValueInterpolation<Vector3> DefaultInterpolation { get; } = Vector3.Lerp;

        protected override ValueApplier<Vector3> CreateApplier(Transform transform) =>
            p => transform.localPosition = p;
    }

    private sealed class SetRotationKeyframesRequest
        : SetEntityKeyframesRequest<Transform, Quaternion>
    {
        public override string Property { get; } = "transform.rotation";

        protected override ValueInterpolation<Quaternion> DefaultInterpolation { get; } =
            Quaternion.Lerp;

        protected override ValueApplier<Quaternion> CreateApplier(Transform transform) =>
            r => transform.localRotation = r;
    }

    private sealed class SetScaleKeyframesRequest : SetEntityKeyframesRequest<Transform, Vector3>
    {
        public override string Property { get; } = "transform.scale";

        protected override ValueInterpolation<Vector3> DefaultInterpolation { get; } = Vector3.Lerp;

        protected override ValueApplier<Vector3> CreateApplier(Transform transform) =>
            p => transform.localScale = p;
    }

    private sealed class SetFocalLengthKeyframesRequest
        : SetEntityKeyframesRequest<PerspectiveSceneCamera, float>
    {
        public override string Property { get; } = "perspective.focalLength";

        protected override ValueInterpolation<float> DefaultInterpolation { get; } = Mathf.Lerp;

        protected override ValueApplier<float> CreateApplier(PerspectiveSceneCamera camera) =>
            f => camera.Perspective = camera.Perspective with { FocalLength = f };
    }

    private sealed class SetSensorSizeKeyframesRequest
        : SetEntityKeyframesRequest<PerspectiveSceneCamera, Vector2>
    {
        public override string Property { get; } = "perspective.sensorSize";

        protected override ValueInterpolation<Vector2> DefaultInterpolation { get; } = Vector2.Lerp;

        protected override ValueApplier<Vector2> CreateApplier(PerspectiveSceneCamera camera) =>
            s => camera.Perspective = camera.Perspective with { SensorSize = s };
    }

    private sealed class SetLensShiftKeyframesRequest
        : SetEntityKeyframesRequest<PerspectiveSceneCamera, Vector2>
    {
        public override string Property { get; } = "perspective.lensShift";

        protected override ValueInterpolation<Vector2> DefaultInterpolation { get; } = Vector2.Lerp;

        protected override ValueApplier<Vector2> CreateApplier(PerspectiveSceneCamera camera) =>
            s => camera.Perspective = camera.Perspective with { LensShift = s };
    }

    private sealed class SetNearClipPlaneKeyframesRequest
        : SetEntityKeyframesRequest<PerspectiveSceneCamera, float>
    {
        public override string Property { get; } = "perspective.nearClipPlane";

        protected override ValueInterpolation<float> DefaultInterpolation { get; } = Mathf.Lerp;

        protected override ValueApplier<float> CreateApplier(PerspectiveSceneCamera camera) =>
            n => camera.Perspective = camera.Perspective with { NearClipPlane = n };
    }

    private sealed class SetFarClipPlaneKeyframesRequest
        : SetEntityKeyframesRequest<PerspectiveSceneCamera, float>
    {
        public override string Property { get; } = "perspective.farClipPlane";

        protected override ValueInterpolation<float> DefaultInterpolation { get; } = Mathf.Lerp;

        protected override ValueApplier<float> CreateApplier(PerspectiveSceneCamera camera) =>
            f => camera.Perspective = camera.Perspective with { FarClipPlane = f };
    }
}
