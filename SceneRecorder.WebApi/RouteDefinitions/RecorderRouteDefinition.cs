using Picalines.OuterWilds.SceneRecorder.Shared.Models;
using Picalines.OuterWilds.SceneRecorder.WebApi.Extensions;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class RecorderRouteDefinition : IApiRouteDefinition
{
    public static RecorderRouteDefinition Instance { get; } = new();

    private RecorderRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var precondition = serverBuilder.UseInPlayableScenePrecondition();

        var outputRecorder = context.OutputRecorder;

        serverBuilder.Map(HttpMethod.Get, "recorder/settings", request =>
        {
            return outputRecorder.Settings is { } sceneSettings
                ? ResponseFabric.Ok(sceneSettings)
                : ResponseFabric.NotFound();
        });

        serverBuilder.Map(HttpMethod.Put, "recorder/settings", request =>
        {
            if (outputRecorder.IsRecording)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            var newSceneSettings = request.ParseContentJson<RecorderSettings>();
            outputRecorder.Settings = newSceneSettings;

            return ResponseFabric.Ok();
        });

        serverBuilder.Map(HttpMethod.Get, "recorder/is_able_to_record", request =>
        {
            return ResponseFabric.Ok(outputRecorder.IsAbleToRecord);
        });

        serverBuilder.Map(HttpMethod.Get, "recorder/frames_recorded", request =>
        {
            return outputRecorder.IsAbleToRecord
                ? ResponseFabric.Ok(outputRecorder.FramesRecorded)
                : ResponseFabric.ServiceUnavailable();
        });

        serverBuilder.Map(HttpMethod.Get, "recorder/enabled", request =>
        {
            return ResponseFabric.Ok(outputRecorder.enabled);
        });

        serverBuilder.Map(HttpMethod.Put, "recorder/enabled", request =>
        {
            var shouldRecord = request.ParseContentJson<bool>();

            if ((shouldRecord, outputRecorder.IsAbleToRecord) is (true, false))
            {
                return ResponseFabric.ServiceUnavailable();
            }

            if (shouldRecord != outputRecorder.IsRecording)
            {
                outputRecorder.enabled = shouldRecord;
            }

            return ResponseFabric.Ok();
        });
    }
}
