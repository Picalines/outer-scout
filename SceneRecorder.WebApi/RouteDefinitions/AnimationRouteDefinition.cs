using SceneRecorder.Recording;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.RouteDefinitions;

using static ResponseFabric;

internal sealed class AnimationRouteDefinition : IApiRouteDefinition
{
    public static AnimationRouteDefinition Instance { get; } = new();

    private AnimationRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var gameScenePrecondition = serverBuilder.UseInPlayableScenePrecondition();
        using var ableToRecordPrecondition = serverBuilder.UsePrecondition(request =>
        {
            return context.OutputRecorder.IsAbleToRecord ? null : ServiceUnavailable();
        });

        MapAnimatorRoutes(
            serverBuilder,
            "free_camera/transform",
            () => context.OutputRecorder.FreeCameraTransformAnimator
        );

        MapAnimatorRoutes(
            serverBuilder,
            "free_camera/camera_info",
            () => context.OutputRecorder.FreeCameraInfoAnimator
        );

        MapAnimatorRoutes(
            serverBuilder,
            "hdri_pivot/transform",
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
        string routeName,
        Func<IAnimator<T>?> getAnimator
    )
    {
        serverBuilder.MapPut(
            $"animation/{routeName}/value",
            (int fromFrame, int toFrame, T newValue) =>
            {
                var animator = getAnimator();
                if (animator is null)
                {
                    return NotFound("animator not found");
                }

                var allFrameNumbers = animator.GetFrameNumbers();

                if (
                    fromFrame > toFrame
                    || !(allFrameNumbers.Contains(fromFrame) && allFrameNumbers.Contains(toFrame))
                )
                {
                    return BadRequest("invalid frame range");
                }

                for (int frame = fromFrame; frame <= toFrame; frame++)
                {
                    animator.SetValueAtFrame(frame, newValue);
                }

                return ResponseFabric.Ok();
            }
        );

        serverBuilder.MapPut(
            $"animation/{routeName}/values",
            (int fromFrame, T[] newValues) =>
            {
                var animator = getAnimator();
                if (animator is null)
                {
                    return NotFound("animator not found");
                }

                var allFrameNumbers = animator.GetFrameNumbers();

                if (allFrameNumbers.Contains(fromFrame) is false)
                {
                    return BadRequest("invalid 'from_frame'");
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
