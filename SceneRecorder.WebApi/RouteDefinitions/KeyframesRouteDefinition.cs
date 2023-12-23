using SceneRecorder.Recording;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.RouteDefinitions;

using static ResponseFabric;

internal sealed class KeyframesRouteDefinition : IApiRouteDefinition
{
    public static KeyframesRouteDefinition Instance { get; } = new();

    private KeyframesRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
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
            (int fromFrame, T[] newValues) =>
            {
                if (getAnimator() is not { } animator)
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
