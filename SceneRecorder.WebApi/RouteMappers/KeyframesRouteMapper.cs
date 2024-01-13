using SceneRecorder.Recording;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using SceneRecorder.WebApi.RouteMappers.DTOs;

namespace SceneRecorder.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class KeyframesRouteMapper : IRouteMapper
{
    public static KeyframesRouteMapper Instance { get; } = new();

    private KeyframesRouteMapper() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IRouteMapper.IContext context)
    {
        using var gameScenePrecondition = serverBuilder.UseInPlayableScenePrecondition();
        using var ableToRecordPrecondition = serverBuilder.UsePrecondition(request =>
        {
            return context.OutputRecorder.IsAbleToRecord ? null : ServiceUnavailable();
        });

        MapAnimatorRoutes(
            serverBuilder,
            "free-camera/transform",
            () => context.OutputRecorder.FreeCameraTransformAnimator
        );

        MapAnimatorRoutes(
            serverBuilder,
            "free-camera/camera-info",
            () => context.OutputRecorder.FreeCameraInfoAnimator
        );

        MapAnimatorRoutes(
            serverBuilder,
            "hdri-pivot/transform",
            () => context.OutputRecorder.HdriTransformAnimator
        );

        MapAnimatorRoutes(
            serverBuilder,
            "time/scale",
            () => context.OutputRecorder.TimeScaleAnimator
        );
    }

    private void MapAnimatorRoutes<T>(
        HttpServerBuilder serverBuilder,
        string routePrefix,
        Func<IAnimator<T>?> getAnimator
    )
    {
        serverBuilder.MapPut(
            $"{routePrefix}/keyframes",
            (SetKeyframesRequest<T> request) =>
            {
                if (getAnimator() is not { } animator)
                {
                    return NotFound("animator not found");
                }

                var newValues = request.Values;
                var fromFrame = request.FromFrame;
                var allFrameNumbers = animator.GetFrameNumbers();

                if (allFrameNumbers.Contains(fromFrame) is false)
                {
                    return BadRequest("invalid start frame");
                }

                var toFrame = fromFrame + newValues.Length - 1;

                if (allFrameNumbers.Contains(toFrame) is false)
                {
                    return BadRequest("frame range out of bounds");
                }

                for (int frame = fromFrame; frame <= toFrame; frame++)
                {
                    animator.SetValueAtFrame(frame, newValues[frame - fromFrame]);
                }

                return Ok();
            }
        );
    }
}
