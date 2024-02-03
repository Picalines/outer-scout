using SceneRecorder.Application.Animation;
using SceneRecorder.Application.Animation.Interpolation;
using SceneRecorder.Application.Animation.ValueApplication;
using SceneRecorder.Application.Extensions;
using SceneRecorder.Application.SceneCameras;
using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.DTOs;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteMappers;

using SceneRecorder.Application.Recording;
using static ResponseFabric;

internal sealed class KeyframesRouteMapper : IRouteMapper
{
    public static KeyframesRouteMapper Instance { get; } = new();

    private KeyframesRouteMapper() { }

    internal sealed record SetTransformKeyframesRequest(
        int FromFrame,
        string RelativeTo,
        TransformDTO[] Values
    );

    internal sealed record SetPerspectiveKeyframesRequest(
        int FromFrame,
        PerspectiveCameraInfoDTO[] Values
    );

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        {
            serverBuilder.MapPut(
                "gameObjects/:name/transform/keyframes",
                PutGameObjectTransformKeyframes
            );

            serverBuilder.MapPut("cameras/:id/transform/keyframes", PutCameraTransformKeyframes);

            serverBuilder.MapPut(
                "cameras/:id/perspective-info/keyframes",
                PutCameraPerspectiveKeyframes
            );
        }
    }

    private static IResponse PutGameObjectTransformKeyframes(
        string name,
        [FromBody] SetTransformKeyframesRequest request,
        SceneRecorder.Builder sceneRecorderBuilder
    )
    {
        var sceneFrameRange = sceneRecorderBuilder.FrameRange;
        var requestFrameRange = IntRange.FromCount(request.FromFrame, request.Values.Length);

        if (sceneFrameRange.Contains(requestFrameRange) is false)
        {
            return BadRequest("invalid frame range");
        }

        if (GameObject.Find(name).OrNull() is not { transform: var targetTransform })
        {
            return NotFound();
        }

        if (GameObject.Find(request.RelativeTo).OrNull() is not { transform: var relativeTo })
        {
            return NotFound();
        }

        return SetTransformKeyframes(sceneRecorderBuilder, request, targetTransform, relativeTo);
    }

    private static IResponse PutCameraTransformKeyframes(
        string id,
        [FromBody] SetTransformKeyframesRequest request,
        SceneRecorder.Builder sceneRecorderBuilder
    )
    {
        var sceneFrameRange = sceneRecorderBuilder.FrameRange;
        var requestFrameRange = IntRange.FromCount(request.FromFrame, request.Values.Length);

        if (sceneFrameRange.Contains(requestFrameRange) is false)
        {
            return BadRequest("invalid frame range");
        }

        if (SceneResource.Find<ISceneCamera>(id) is not { Value: var camera })
        {
            return NotFound();
        }

        if (GameObject.Find(request.RelativeTo).OrNull() is not { transform: var relativeTo })
        {
            return NotFound();
        }

        return SetTransformKeyframes(sceneRecorderBuilder, request, camera.Transform, relativeTo);
    }

    private static IResponse PutCameraPerspectiveKeyframes(
        string id,
        [FromBody] SetPerspectiveKeyframesRequest request,
        SceneRecorder.Builder sceneRecorderBuilder
    )
    {
        var sceneFrameRange = sceneRecorderBuilder.FrameRange;
        var requestFrameRange = IntRange.FromCount(request.FromFrame, request.Values.Length);

        if (sceneFrameRange.Contains(requestFrameRange) is false)
        {
            return BadRequest("invalid frame range");
        }

        if (
            SceneResource.Find<ISceneCamera>(id)
            is not { Value: PerspectiveSceneCamera { gameObject: var gameObject } camera }
        )
        {
            return NotFound();
        }

        if (
            gameObject.GetResource<Animator<PerspectiveCameraInfo>>() is not { Value: var animator }
        )
        {
            var keyframes = new KeyframeStorage<PerspectiveCameraInfo>(sceneFrameRange);

            var valueApplier = ValueApplier.Lambda<PerspectiveCameraInfo>(newPerspective =>
                camera.PerspectiveInfo = newPerspective
            );

            gameObject.AddResource(
                animator = new Animator<PerspectiveCameraInfo>()
                {
                    Keyframes = keyframes,
                    ValueApplier = valueApplier,
                    Interpolation = ConstantInterpolation<PerspectiveCameraInfo>.Instance
                }
            );
        }

        return SetKeyframes(
            animator,
            request.FromFrame,
            request.Values.Select(perspectiveDto => perspectiveDto.ToCameraInfo())
        );
    }

    private static IResponse SetKeyframes<T>(
        Animator<T> animator,
        int fromFrame,
        IEnumerable<T> newValues
    )
    {
        foreach (var (index, value) in newValues.Indexed())
        {
            var frame = fromFrame + index;
            animator.Keyframes.SetKeyframe(frame, value);
        }

        return Ok();
    }

    private static IResponse SetTransformKeyframes(
        SceneRecorder.Builder sceneRecorderBuilder,
        SetTransformKeyframesRequest request,
        Transform targetTransform,
        Transform relativeTo
    )
    {
        var sceneFrameRange = sceneRecorderBuilder.FrameRange;
        var requestFrameRange = IntRange.FromCount(request.FromFrame, request.Values.Length);

        if (sceneFrameRange.Contains(requestFrameRange) is false)
        {
            return BadRequest("invalid frame range");
        }

        var gameObject = targetTransform.gameObject;

        if (gameObject.GetResource<Animator<LocalTransform>>() is not { Value: var animator })
        {
            var keyframes = new KeyframeStorage<LocalTransform>(sceneFrameRange);

            var valueApplier = ValueApplier.Lambda<LocalTransform>(newTransform =>
            {
                if (targetTransform != null)
                    targetTransform.Apply(newTransform);
            });

            gameObject.AddResource(
                animator = new Animator<LocalTransform>()
                {
                    Keyframes = keyframes,
                    ValueApplier = valueApplier,
                    Interpolation = ConstantInterpolation<LocalTransform>.Instance
                }
            );
        }

        return SetKeyframes(
            animator,
            request.FromFrame,
            request.Values.Select(transformDto => transformDto.ToLocalTransform(relativeTo))
        );
    }
}
