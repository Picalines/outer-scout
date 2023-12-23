using SceneRecorder.Recording;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.RouteDefinitions;

internal sealed class AnimationRouteDefinition : IApiRouteDefinition
{
    public static AnimationRouteDefinition Instance { get; } = new();

    private AnimationRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var gameScenePrecondition = serverBuilder.UseInPlayableScenePrecondition();
        using var ableToRecordPrecondition = serverBuilder.UsePrecondition(request =>
        {
            return context.OutputRecorder.IsAbleToRecord is false
                ? ResponseFabric.ServiceUnavailable()
                : null;
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
        serverBuilder.Map(
            HttpMethod.Put,
            $"animation/{routeName}/value",
            (Request request, int from_frame, int to_frame) =>
            {
                var animator = getAnimator();
                if (animator is null)
                {
                    return ResponseFabric.NotFound("animator not found");
                }

                var allFrameNumbers = animator.GetFrameNumbers();

                if (
                    from_frame > to_frame
                    || !(allFrameNumbers.Contains(from_frame) && allFrameNumbers.Contains(to_frame))
                )
                {
                    return ResponseFabric.BadRequest("invalid frame range");
                }

                var newValue = request.ParseContentJson<T>();

                for (int frame = from_frame; frame <= to_frame; frame++)
                {
                    animator.SetValueAtFrame(frame, newValue);
                }

                return ResponseFabric.Ok();
            }
        );

        serverBuilder.Map(
            HttpMethod.Put,
            $"animation/{routeName}/values",
            (Request request, int from_frame) =>
            {
                var animator = getAnimator();
                if (animator is null)
                {
                    return ResponseFabric.NotFound("animator not found");
                }

                var allFrameNumbers = animator.GetFrameNumbers();

                if (allFrameNumbers.Contains(from_frame) is false)
                {
                    return ResponseFabric.BadRequest("invalid 'from_frame'");
                }

                var newValues = request.ParseContentJson<T[]>();
                var toFrame = from_frame + newValues.Length - 1;

                if (allFrameNumbers.Contains(toFrame) is false)
                {
                    return ResponseFabric.BadRequest("frame range out of bounds");
                }

                for (int frame = from_frame; frame <= toFrame; frame++)
                {
                    animator.SetValueAtFrame(frame, newValues[frame - from_frame]);
                }

                return ResponseFabric.Ok();
            }
        );
    }
}
