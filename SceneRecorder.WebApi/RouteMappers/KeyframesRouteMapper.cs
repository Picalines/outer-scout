using Newtonsoft.Json;
using SceneRecorder.Application.Animation;
using SceneRecorder.Application.SceneCameras;
using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Validation;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using SceneRecorder.WebApi.Services;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteMappers;

using SceneRecorder.Application.Recording;
using static ResponseFabric;

internal sealed class KeyframesRouteMapper : IRouteMapper
{
    public static KeyframesRouteMapper Instance { get; } = new();

    private KeyframesRouteMapper() { }

    private sealed class KeyframeDTO<T>
    {
        public required T Value { get; init; }
    }

    private sealed class SetKeyframesRequest<T>
    {
        public required IReadOnlyDictionary<int, KeyframeDTO<T>> Keyframes { get; init; }
    }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithSceneCreatedFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            serverBuilder.MapPut("gameObjects/:name/:property/keyframes", PutGameObjectKeyframes);

            serverBuilder.MapPut("cameras/:id/:property/keyframes", PutCameraKeyframes);
        }
    }

    private static IResponse PutGameObjectKeyframes(
        [FromUrl] string name,
        [FromUrl] string property,
        Request request,
        JsonSerializer jsonSerializer,
        GameObjectRepository gameObjects,
        ApiResourceRepository resources
    )
    {
        if (
            resources.GlobalContainer.GetResource<SceneRecorder.Builder>()
            is not { } sceneRecorderBuilder
        )
        {
            return ServiceUnavailable();
        }

        if (property is not ("position" or "rotation" or "scale"))
        {
            return NotFound($"gameObject property '{property}' cannot be animated");
        }

        if (gameObjects.FindOrNull(name) is not { transform: var transform } gameObject)
        {
            return NotFound();
        }

        var frameRange = sceneRecorderBuilder.FrameRange;

        if (resources.ContainerOf(gameObject).GetResource<IAnimator>(property) is not { } animator)
        {
            animator =
                CreateTransformAnimator(property, frameRange, transform)
                ?? throw new NotImplementedException();

            resources.ContainerOf(gameObject).AddResource(property, animator);

            sceneRecorderBuilder.WithAnimator(animator);
        }

        return PutKeyframes(request, jsonSerializer, frameRange, animator);
    }

    private static IResponse PutCameraKeyframes(
        [FromUrl] string id,
        [FromUrl] string property,
        Request request,
        JsonSerializer jsonSerializer,
        GameObjectRepository gameObjects,
        ApiResourceRepository resources
    )
    {
        if (
            resources.GlobalContainer.GetResource<SceneRecorder.Builder>()
            is not { } sceneRecorderBuilder
        )
        {
            return ServiceUnavailable();
        }

        if (property is not ("position" or "rotation" or "perspective"))
        {
            return NotFound($"camera property '{property}' cannot be animated");
        }

        if (
            resources.GlobalContainer.GetResource<ISceneCamera>(id)
            is not { Transform: { gameObject: var gameObject } transform } camera
        )
        {
            return NotFound($"camera '{id}' not found");
        }

        if ((property, camera) is ("perspective", not PerspectiveSceneCamera))
        {
            return BadRequest($"camera '{id}' is not perspective");
        }

        var frameRange = sceneRecorderBuilder.FrameRange;

        if (resources.ContainerOf(gameObject).GetResource<IAnimator>(property) is not { } animator)
        {
            animator =
                CreateTransformAnimator(property, frameRange, transform)
                ?? CreatePerspectiveAnimator(frameRange, (PerspectiveSceneCamera)camera);

            resources.ContainerOf(gameObject).AddResource(property, animator);

            sceneRecorderBuilder.WithAnimator(animator);
        }

        return PutKeyframes(request, jsonSerializer, frameRange, animator);
    }

    private static IAnimator? CreateTransformAnimator(
        string property,
        IntRange frameRange,
        Transform transform
    )
    {
        return property switch
        {
            "position"
                => new Animator<Vector3>()
                {
                    Keyframes = new KeyframeStorage<Vector3>(frameRange),
                    ValueApplier = p => transform.localPosition = p,
                    Interpolation = Vector3.Lerp
                },

            "rotation"
                => new Animator<Quaternion>()
                {
                    Keyframes = new KeyframeStorage<Quaternion>(frameRange),
                    ValueApplier = r => transform.localRotation = r,
                    Interpolation = Quaternion.Slerp
                },

            "scale"
                => new Animator<Vector3>()
                {
                    Keyframes = new KeyframeStorage<Vector3>(frameRange),
                    ValueApplier = s => transform.localScale = s,
                    Interpolation = Vector3.Lerp
                },

            _ => null,
        };
    }

    private static Animator<CameraPerspective> CreatePerspectiveAnimator(
        IntRange frameRange,
        PerspectiveSceneCamera camera
    )
    {
        return new Animator<CameraPerspective>()
        {
            Keyframes = new KeyframeStorage<CameraPerspective>(frameRange),
            ValueApplier = p => camera.Perspective = p,
            Interpolation = ConstantInterpolation<CameraPerspective>.Interpolate
        };
    }

    private static IEnumerable<int> StoreKeyframes<T>(
        IntRange sceneFrameRange,
        KeyframeStorage<T> keyframes,
        SetKeyframesRequest<T> request
    )
    {
        var invalidFrames = new HashSet<int>();

        foreach (var (frame, keyframe) in request.Keyframes)
        {
            if (sceneFrameRange.Contains(frame) is false)
            {
                invalidFrames.Add(frame);
                continue;
            }

            keyframes.SetKeyframe(frame, keyframe.Value);
        }

        return invalidFrames;
    }

    private static IResponse PutKeyframes<T>(
        Request request,
        JsonSerializer jsonSerializer,
        IntRange frameRange,
        Animator<T> animator
    )
    {
        SetKeyframesRequest<T>? setKeyframesRequest = null;

        using (var jsonTextReader = new JsonTextReader(request.BodyReader))
        {
            setKeyframesRequest = jsonSerializer.Deserialize<SetKeyframesRequest<T>>(
                jsonTextReader
            );

            if (setKeyframesRequest is null)
            {
                return BadRequest("invalid body");
            }
        }

        var invalidKeyframes = StoreKeyframes(frameRange, animator.Keyframes, setKeyframesRequest);

        return Ok(new { invalidKeyframes });
    }

    private static IResponse PutKeyframes(
        Request request,
        JsonSerializer jsonSerializer,
        IntRange sceneFrameRange,
        IAnimator animator
    )
    {
        var concreteAnimatorType = animator.GetType();

        concreteAnimatorType
            .Throw(e => new NotImplementedException(e))
            .If(concreteAnimatorType.GetGenericTypeDefinition() != typeof(Animator<>));

        var propertyType = concreteAnimatorType.GenericTypeArguments[0];

        var response = ((Delegate)PutKeyframes<object>)
            .Method.GetGenericMethodDefinition()
            .MakeGenericMethod(propertyType)
            .Invoke(null, [request, jsonSerializer, sceneFrameRange, animator]);

        return (IResponse)response;
    }
}
