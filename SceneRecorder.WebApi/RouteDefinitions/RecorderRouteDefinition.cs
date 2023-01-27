using Picalines.OuterWilds.SceneRecorder.WebApi.Http;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class RecorderRouteDefinition : IApiRouteDefinition
{
    public static RecorderRouteDefinition Instance { get; } = new();

    private RecorderRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        var outputRecorder = context.OutputRecorder;

        serverBuilder.MapGet("recorder/is_able_to_record", request =>
        {
            return ResponseFabric.Ok(outputRecorder.IsAbleToRecord);
        });

        serverBuilder.MapGet("recorder/frames_recorded", request =>
        {
            return outputRecorder switch
            {
                { IsAbleToRecord: false } => ResponseFabric.ServiceUnavailable(),
                { IsAbleToRecord: true } => ResponseFabric.Ok(outputRecorder.FramesRecorded),
            };
        });

        serverBuilder.MapGet("recorder/enabled", request =>
        {
            return ResponseFabric.Ok(outputRecorder.enabled);
        });

        serverBuilder.MapPatch("recorder?{enabled:bool}", request =>
        {
            var shouldRecord = request.GetQueryParameter<bool>("enabled");

            if ((shouldRecord, outputRecorder.IsAbleToRecord) is (true, false))
            {
                return ResponseFabric.ServiceUnavailable();
            }

            if (shouldRecord == outputRecorder.IsRecording)
            {
                return ResponseFabric.NotModified();
            }

            outputRecorder.enabled = shouldRecord;

            return ResponseFabric.Ok();
        });
    }
}
