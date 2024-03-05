using JsonSubTypes;
using Newtonsoft.Json;
using SceneRecorder.Application.Animation;
using SceneRecorder.Application.SceneCameras;
using SceneRecorder.Domain;
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

            foreach (var (frame, keyframe) in Keyframes)
            {
                keyframeStorage.SetKeyframe(frame, keyframe.Value);
            }
        }
    }

    private abstract class SetSceneKeyframesRequest<T> : SetKeyframesRequest<T>, IAnimatorFactory
    {
        protected abstract ValueApplier<T> ValueApplier { get; }

        protected abstract Interpolation<T> Interpolation { get; }

        public IAnimator CreateAnimator(IntRange frameRange)
        {
            return new Animator<T>()
            {
                Keyframes = new KeyframeStorage<T>(frameRange),
                ValueApplier = ValueApplier,
                Interpolation = Interpolation
            };
        }
    }

    private abstract class SetEntityKeyframesRequest<E, T>
        : SetKeyframesRequest<T>,
            IAnimatorFactory<E>
    {
        protected abstract ValueApplier<T> CreateApplier(E entity);

        protected abstract Interpolation<T> Interpolation { get; }

        public IAnimator CreateAnimator(E entity, IntRange frameRange)
        {
            return new Animator<T>()
            {
                Keyframes = new KeyframeStorage<T>(frameRange),
                ValueApplier = CreateApplier(entity),
                Interpolation = Interpolation
            };
        }
    }

    private sealed class SetTimeScaleKeyframesRequest : SetSceneKeyframesRequest<float>
    {
        public override string Property { get; } = "time.scale";

        protected override ValueApplier<float> ValueApplier { get; } = s => Time.timeScale = s;

        protected override Interpolation<float> Interpolation { get; } = Mathf.Lerp;
    }

    private sealed class SetPositionKeyframesRequest : SetEntityKeyframesRequest<Transform, Vector3>
    {
        public override string Property { get; } = "transform.position";

        protected override Interpolation<Vector3> Interpolation { get; } = Vector3.Lerp;

        protected override ValueApplier<Vector3> CreateApplier(Transform transform) =>
            p => transform.localPosition = p;
    }

    private sealed class SetRotationKeyframesRequest
        : SetEntityKeyframesRequest<Transform, Quaternion>
    {
        public override string Property { get; } = "transform.rotation";

        protected override Interpolation<Quaternion> Interpolation { get; } = Quaternion.Lerp;

        protected override ValueApplier<Quaternion> CreateApplier(Transform transform) =>
            r => transform.localRotation = r;
    }

    private sealed class SetScaleKeyframesRequest : SetEntityKeyframesRequest<Transform, Vector3>
    {
        public override string Property { get; } = "transform.scale";

        protected override Interpolation<Vector3> Interpolation { get; } = Vector3.Lerp;

        protected override ValueApplier<Vector3> CreateApplier(Transform transform) =>
            p => transform.localScale = p;
    }

    private sealed class SetFocalLengthKeyframesRequest
        : SetEntityKeyframesRequest<PerspectiveSceneCamera, float>
    {
        public override string Property { get; } = "perspective.focalLength";

        protected override Interpolation<float> Interpolation { get; } = Mathf.Lerp;

        protected override ValueApplier<float> CreateApplier(PerspectiveSceneCamera camera) =>
            f => camera.Perspective = camera.Perspective with { FocalLength = f };
    }

    private sealed class SetSensorSizeKeyframesRequest
        : SetEntityKeyframesRequest<PerspectiveSceneCamera, Vector2>
    {
        public override string Property { get; } = "perspective.sensorSize";

        protected override Interpolation<Vector2> Interpolation { get; } = Vector2.Lerp;

        protected override ValueApplier<Vector2> CreateApplier(PerspectiveSceneCamera camera) =>
            s => camera.Perspective = camera.Perspective with { SensorSize = s };
    }

    private sealed class SetLensShiftKeyframesRequest
        : SetEntityKeyframesRequest<PerspectiveSceneCamera, Vector2>
    {
        public override string Property { get; } = "perspective.lensShift";

        protected override Interpolation<Vector2> Interpolation { get; } = Vector2.Lerp;

        protected override ValueApplier<Vector2> CreateApplier(PerspectiveSceneCamera camera) =>
            s => camera.Perspective = camera.Perspective with { LensShift = s };
    }

    private sealed class SetNearClipPlaneKeyframesRequest
        : SetEntityKeyframesRequest<PerspectiveSceneCamera, float>
    {
        public override string Property { get; } = "perspective.nearClipPlane";

        protected override Interpolation<float> Interpolation { get; } = Mathf.Lerp;

        protected override ValueApplier<float> CreateApplier(PerspectiveSceneCamera camera) =>
            n => camera.Perspective = camera.Perspective with { NearClipPlane = n };
    }

    private sealed class SetFarClipPlaneKeyframesRequest
        : SetEntityKeyframesRequest<PerspectiveSceneCamera, float>
    {
        public override string Property { get; } = "perspective.farClipPlane";

        protected override Interpolation<float> Interpolation { get; } = Mathf.Lerp;

        protected override ValueApplier<float> CreateApplier(PerspectiveSceneCamera camera) =>
            f => camera.Perspective = camera.Perspective with { FarClipPlane = f };
    }
}
