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
        serverBuilder.MapPut($"animation/{routeName}/value_at_frame/{{frame_index:int}}", request =>
        {
            var frameIndex = request.GetRouteParameter<int>("frame_index");
            if (frameIndex < 0)
            {
                return ResponseFabric.BadRequest();
            }

            var animator = getAnimator();
            if (frameIndex >= animator.FrameCount)
            {
                return ResponseFabric.BadRequest();
            }

            var newValue = request.ParseContentJson<T>();
            animator.SetValueAtFrame(frameIndex, newValue);

            return ResponseFabric.Ok();
        });

        serverBuilder.MapPut($"animation/{routeName}/values_from_frame/{{start_frame_index:int}}", request =>
        {
            var startFrameIndex = request.GetRouteParameter<int>("start_frame_index");
            if (startFrameIndex < 0)
            {
                return ResponseFabric.BadRequest();
            }

            var animator = getAnimator();
            var newValues = request.ParseContentJson<T[]>();

            if ((startFrameIndex + newValues.Length) > animator.FrameCount)
            {
                return ResponseFabric.BadRequest();
            }

            for (int frameOffset = 0; frameOffset < newValues.Length; frameOffset++)
            {
                animator.SetValueAtFrame(startFrameIndex + frameOffset, newValues[frameOffset]);
            }

            return ResponseFabric.Ok();
        });
    }
}
