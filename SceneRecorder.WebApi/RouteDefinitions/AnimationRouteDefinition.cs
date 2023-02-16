using Picalines.OuterWilds.SceneRecorder.Recording;
using Picalines.OuterWilds.SceneRecorder.WebApi.Extensions;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class AnimationRouteDefinition : IApiRouteDefinition
{
    public static AnimationRouteDefinition Instance { get; } = new();

    private AnimationRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var gameScenePrecondition = serverBuilder.UseInGameScenePrecondition();
        using var ableToRecordPrecondition = serverBuilder.UsePrecondition(request =>
        {
            return context.OutputRecorder.IsAbleToRecord is false
                ? ResponseFabric.ServiceUnavailable()
                : null;
        });

        MapAnimatorRoutes(serverBuilder, "free_camera/transform", () => context.OutputRecorder.FreeCameraTransformAnimator!);
        MapAnimatorRoutes(serverBuilder, "free_camera/camera_info", () => context.OutputRecorder.FreeCameraInfoAnimator!);
        MapAnimatorRoutes(serverBuilder, "hdri_pivot/transform", () => context.OutputRecorder.HdriTransformAnimator!);
        MapAnimatorRoutes(serverBuilder, "time/scale", () => context.OutputRecorder.TimeScaleAnimator!);
    }

    private void MapAnimatorRoutes<T>(HttpServerBuilder serverBuilder, string routeName, Func<IAnimator<T>> getAnimator)
    {
        serverBuilder.MapPut($"animation/{routeName}/value?{{at_frame:int}}", request =>
        {
            var animator = getAnimator();
            var frame = request.GetQueryParameter<int>("at_frame");

            if (animator.GetFrameNumbers().Contains(frame) is false)
            {
                return ResponseFabric.BadRequest();
            }

            var newValue = request.ParseContentJson<T>();
            animator.SetValueAtFrame(frame, newValue);

            return ResponseFabric.Ok();
        });

        serverBuilder.MapPut($"animation/{routeName}/value?{{from_frame:int}}&{{to_frame:int}}", request =>
        {
            var fromFrame = request.GetQueryParameter<int>("from_frame");
            var toFrame = request.GetQueryParameter<int>("to_frame");

            var animator = getAnimator();
            var allFameNumbers = animator.GetFrameNumbers();

            if (fromFrame > toFrame
                || (allFameNumbers.Contains(fromFrame), allFameNumbers.Contains(toFrame)) is not (true, true))
            {
                return ResponseFabric.BadRequest("invalid frame range");
            }

            var newValue = request.ParseContentJson<T>();

            for (int frame = fromFrame; frame <= toFrame; frame++)
            {
                animator.SetValueAtFrame(frame, newValue);
            }

            return ResponseFabric.Ok();
        });

        serverBuilder.MapPut($"animation/{routeName}/values?{{from_frame:int}}", request =>
        {
            var fromFrame = request.GetQueryParameter<int>("from_frame");
            var animator = getAnimator();
            var allFrameNumbers = animator.GetFrameNumbers();

            if (allFrameNumbers.Contains(fromFrame) is false)
            {
                return ResponseFabric.BadRequest("invalid 'from_frame'");
            }

            var newValues = request.ParseContentJson<T[]>();
            var toFrame = fromFrame + newValues.Length - 1;

            if (allFrameNumbers.Contains(toFrame) is false)
            {
                return ResponseFabric.BadRequest("frame range out of bounds");
            }

            for (int frame = fromFrame; frame <= toFrame; frame++)
            {
                animator.SetValueAtFrame(frame, newValues[frame - fromFrame]);
            }

            return ResponseFabric.Ok();
        });
    }
}
