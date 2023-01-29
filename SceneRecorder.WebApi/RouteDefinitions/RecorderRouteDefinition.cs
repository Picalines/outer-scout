using Picalines.OuterWilds.SceneRecorder.Json;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class RecorderRouteDefinition : IApiRouteDefinition
{
    public static RecorderRouteDefinition Instance { get; } = new();

    private RecorderRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        var outputRecorder = context.OutputRecorder;

        serverBuilder.MapGet("recorder/settings", request =>
        {
            return outputRecorder.Settings is { } sceneSettings
                ? ResponseFabric.Ok(sceneSettings)
                : ResponseFabric.NotFound();
        });

        serverBuilder.MapPut("recorder/settings", request =>
        {
            if (outputRecorder.IsRecording)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            var newSceneSettings = request.ParseContentJson<RecorderSettings>();
            outputRecorder.Settings = newSceneSettings;

            return ResponseFabric.Ok();
        });

        serverBuilder.MapPut("recorder/output_directory", request =>
        {
            if (outputRecorder.IsRecording)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            outputRecorder.OutputDirectory = request.Content;
            return ResponseFabric.Ok();
        });

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

        serverBuilder.MapPut("recorder/enabled", request =>
        {
            var shouldRecord = request.ParseContentJson<bool>();

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
