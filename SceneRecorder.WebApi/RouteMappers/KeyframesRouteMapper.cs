using SceneRecorder.Application.Animation;
using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.DTOs;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class KeyframesRouteMapper : IRouteMapper
{
    public static KeyframesRouteMapper Instance { get; } = new();

    private KeyframesRouteMapper() { }

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
        [FromBody] SetKeyframesRequest<TransformDTO> request
    )
    {
        if (GameObject.Find(name).OrNull() is not { } gameObject)
        {
            return NotFound();
        }

        // TODO: get or create animator, save it to Builder
        Animator<TransformDTO> animator = null!;

        return SetKeyframes(animator, request);
    }

    private static IResponse PutCameraTransformKeyframes(
        string id,
        [FromBody] SetKeyframesRequest<TransformDTO> request
    )
    {
        throw new NotImplementedException();
    }

    private static IResponse PutCameraPerspectiveKeyframes(
        string id,
        [FromBody] SetKeyframesRequest<PerspectiveSceneCameraDTO> request
    )
    {
        throw new NotImplementedException();
    }

    private static IResponse SetKeyframes<T>(Animator<T> animator, SetKeyframesRequest<T> request)
    {
        var newValues = request.Values;
        var fromFrame = request.FromFrame;

        var animatorFrameRange = animator.Keyframes.FrameRange;
        var requestFrameRange = IntRange.FromCount(request.FromFrame, newValues.Length);

        if (animatorFrameRange.Contains(requestFrameRange) is false)
        {
            return BadRequest("invalid frame range");
        }

        foreach (var (index, frame) in requestFrameRange.Indexed())
        {
            animator.Keyframes.SetKeyframe(frame, newValues[index]);
        }

        return Ok();
    }
}
