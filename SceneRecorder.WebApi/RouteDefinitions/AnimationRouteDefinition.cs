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
        MapAnimatorRoutes(serverBuilder, "hdri_pivot/transform", () => context.OutputRecorder.HdriTransformAnimator!);
    }

    private void MapAnimatorRoutes<T>(HttpServerBuilder serverBuilder, string routeName, Func<IAnimator<T>> getAnimator)
    {
        serverBuilder.MapGet($"animation/{routeName}/frame_count", request =>
        {
            var animator = getAnimator();
            return ResponseFabric.Ok(animator.FrameCount);
        });

        serverBuilder.MapPut($"animation/{routeName}/frame_count?{{new_count:int}}", request =>
        {
            var newCount = request.GetRouteParameter<int>("new_count");
            if (newCount <= 0)
            {
                return ResponseFabric.BadRequest();
            }

            var animator = getAnimator();
            animator.FrameCount = newCount;

            return ResponseFabric.Ok();
        });

        serverBuilder.MapPut($"animation/{routeName}/value_at_frame/{{frame_index:int}}", request =>
        {
            var frameIndex = request.GetRouteParameter<int>("frame_index");
            if (frameIndex < 0)
            {
                return ResponseFabric.BadRequest();
            }

            var newValue = request.ParseContentJson<T>();

            var animator = getAnimator();
            animator.SetValueAtFrame(frameIndex, newValue);

            return ResponseFabric.Ok();
        });

        serverBuilder.MapPut($"animation/{routeName}/value_at_frame/{{start_frame_index:int}}/bulk", request =>
        {
            var startFrameIndex = request.GetRouteParameter<int>("start_frame_index");
            if (startFrameIndex < 0)
            {
                return ResponseFabric.BadRequest();
            }

            var newValues = request.ParseContentJson<T[]>();

            var animator = getAnimator();
            for (int frameOffset = 0; frameOffset < newValues.Length; frameOffset++)
            {
                animator.SetValueAtFrame(startFrameIndex + frameOffset, newValues[frameOffset]);
            }

            return ResponseFabric.Ok();
        });
    }
}
