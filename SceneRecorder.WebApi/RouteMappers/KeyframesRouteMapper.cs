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
using SceneRecorder.WebApi.Services;
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

        if (gameObjects.FindOrNull(name) is not { transform: var targetTransform })
        {
            return NotFound();
        }

        return SetKeyframes(
            sceneRecorderBuilder.FrameRange,
            animator: GetTransformAnimator(resources, sceneRecorderBuilder, targetTransform),
            request,
            t => ConvertTransfromDTO(gameObjects, t)
        );
    }

    private static IResponse PutCameraTransformKeyframes(
        string id,
        [FromBody] SetKeyframesRequest<TransformDTO> request,
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

        if (
            resources.GlobalContainer.GetResource<ISceneCamera>(id)
            is not { Transform: var targetTransform }
        )
        {
            return NotFound();
        }

        return SetKeyframes(
            sceneRecorderBuilder.FrameRange,
            animator: GetTransformAnimator(resources, sceneRecorderBuilder, targetTransform),
            request,
            t => ConvertTransfromDTO(gameObjects, t)
        );
    }

    private static IResponse PutCameraPerspectiveKeyframes(
        string id,
        [FromBody] SetKeyframesRequest<CameraPerspectiveDTO> request,
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

        if (
            resources.GlobalContainer.GetResource<ISceneCamera>(id)
            is not PerspectiveSceneCamera camera
        )
        {
            return NotFound();
        }

        return SetKeyframes(
            sceneRecorderBuilder.FrameRange,
            animator: GetPerspectiveAnimator(resources, sceneRecorderBuilder, camera),
            request,
            perspectiveDto => perspectiveDto.ToPerspective()
        );
    }

    private static LocalTransform ConvertTransfromDTO(
        GameObjectRepository gameObjects,
        TransformDTO transformDTO
    )
    {
        return transformDTO.ToLocalTransform(
            transformDTO.Parent is { } parentName
            && gameObjects.FindOrNull(parentName) is { transform: var parent }
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
        ApiResourceRepository resources,
        SceneRecorder.Builder sceneRecorderBuilder,
        Transform targetTransform
    )
    {
        var gameObject = targetTransform.gameObject;

        if (
            resources.ContainerOf(gameObject).GetResource<Animator<LocalTransform>>("transform")
            is not { } animator
        )
        {
            var keyframes = new KeyframeStorage<LocalTransform>(sceneRecorderBuilder.FrameRange);

            var valueApplier = ValueApplier.Lambda<LocalTransform>(newTransform =>
            {
                if (targetTransform != null)
                    targetTransform.Apply(newTransform);
            });

            animator = new Animator<LocalTransform>()
            {
                Keyframes = keyframes,
                ValueApplier = valueApplier,
                Interpolation = ConstantInterpolation<LocalTransform>.Instance
            };

            resources.ContainerOf(gameObject).AddResource("transform", animator);

            sceneRecorderBuilder.WithAnimator(animator);
        }

        return animator;
    }

    private static Animator<CameraPerspective> GetPerspectiveAnimator(
        ApiResourceRepository resources,
        SceneRecorder.Builder sceneRecorderBuilder,
        PerspectiveSceneCamera camera
    )
    {
        var gameObject = camera.gameObject;

        if (
            resources
                .ContainerOf(gameObject)
                .GetResource<Animator<CameraPerspective>>("perspective")
            is not { } animator
        )
        {
            var keyframes = new KeyframeStorage<CameraPerspective>(sceneRecorderBuilder.FrameRange);

            var valueApplier = ValueApplier.Lambda<CameraPerspective>(newPerspective =>
                camera.Perspective = newPerspective
            );

            animator = new Animator<CameraPerspective>()
            {
                Keyframes = keyframes,
                ValueApplier = valueApplier,
                Interpolation = ConstantInterpolation<CameraPerspective>.Instance
            };

            resources.ContainerOf(gameObject).AddResource("perspective", animator);

            sceneRecorderBuilder.WithAnimator(animator);
        }

        return animator;
    }
}
