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

    private sealed class SetKeyframesRequest<T>
    {
        public required int FromFrame { get; init; }

        public required T[] Values { get; init; }
    }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithSceneCreatedFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            serverBuilder.MapPut(
                "gameObjects/:name/transform/keyframes",
                PutGameObjectTransformKeyframes
            );

            serverBuilder.MapPut("cameras/:id/transform/keyframes", PutCameraTransformKeyframes);

            serverBuilder.MapPut(
                "cameras/:id/perspective/keyframes",
                PutCameraPerspectiveKeyframes
            );
        }
    }

    private static IResponse PutGameObjectTransformKeyframes(
        string name,
        [FromBody] SetKeyframesRequest<TransformDTO> request,
        SceneRecorder.Builder sceneRecorderBuilder
    )
    {
        if (GameObject.Find(name).OrNull() is not { transform: var targetTransform })
        {
            return NotFound();
        }

        var sceneFrameRange = sceneRecorderBuilder.FrameRange;

        return SetKeyframes(
            sceneFrameRange,
            animator: GetTransformAnimator(sceneFrameRange, targetTransform),
            request,
            ConvertTransfromDTO
        );
    }

    private static IResponse PutCameraTransformKeyframes(
        string id,
        [FromBody] SetKeyframesRequest<TransformDTO> request,
        SceneRecorder.Builder sceneRecorderBuilder
    )
    {
        if (SceneResource.Find<ISceneCamera>(id) is not { Value.Transform: var targetTransform })
        {
            return NotFound();
        }

        var sceneFrameRange = sceneRecorderBuilder.FrameRange;

        return SetKeyframes(
            sceneFrameRange,
            animator: GetTransformAnimator(sceneFrameRange, targetTransform),
            request,
            ConvertTransfromDTO
        );
    }

    private static IResponse PutCameraPerspectiveKeyframes(
        string id,
        [FromBody] SetKeyframesRequest<CameraPerspectiveDTO> request,
        SceneRecorder.Builder sceneRecorderBuilder
    )
    {
        if (SceneResource.Find<ISceneCamera>(id) is not { Value: PerspectiveSceneCamera camera })
        {
            return NotFound();
        }

        var sceneFrameRange = sceneRecorderBuilder.FrameRange;

        return SetKeyframes(
            sceneFrameRange,
            animator: GetPerspectiveAnimator(sceneFrameRange, camera),
            request,
            perspectiveDto => perspectiveDto.ToPerspective()
        );
    }

    private static LocalTransform ConvertTransfromDTO(TransformDTO transformDTO)
    {
        return transformDTO.ToLocalTransform(
            parent: transformDTO.Parent is { } parentName
            && GameObject.Find(parentName).OrNull() is { transform: var parent }
                ? parent
                : null
        );
    }

    private static IResponse SetKeyframes<T, D>(
        IntRange sceneFrameRange,
        Animator<T> animator,
        SetKeyframesRequest<D> request,
        Func<D, T?> convertDto
    )
    {
        var requestFrameRange = IntRange.FromCount(request.FromFrame, request.Values.Length);

        if (sceneFrameRange.Contains(requestFrameRange) is false)
        {
            return BadRequest("invalid frame range");
        }

        foreach (var (index, valueDto) in request.Values.Indexed())
        {
            var frame = request.FromFrame + index;

            if (convertDto(valueDto) is not { } value)
            {
                return BadRequest(new { Frame = frame, Message = "invalid value" });
            }

            animator.Keyframes.SetKeyframe(frame, value);
        }

        return Ok();
    }

    private static Animator<LocalTransform> GetTransformAnimator(
        IntRange sceneFrameRange,
        Transform targetTransform
    )
    {
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

        return animator;
    }

    private static Animator<CameraPerspective> GetPerspectiveAnimator(
        IntRange sceneFrameRange,
        PerspectiveSceneCamera camera
    )
    {
        var gameObject = camera.gameObject;

        if (gameObject.GetResource<Animator<CameraPerspective>>() is not { Value: var animator })
        {
            var keyframes = new KeyframeStorage<CameraPerspective>(sceneFrameRange);

            var valueApplier = ValueApplier.Lambda<CameraPerspective>(newPerspective =>
                camera.Perspective = newPerspective
            );

            gameObject.AddResource(
                animator = new Animator<CameraPerspective>()
                {
                    Keyframes = keyframes,
                    ValueApplier = valueApplier,
                    Interpolation = ConstantInterpolation<CameraPerspective>.Instance
                }
            );
        }

        return animator;
    }
}
