using System.Collections;
using SceneRecorder.Recording.Recorders;
using SceneRecorder.Shared.Models;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.RouteDefinitions;

using static ResponseFabric;

internal sealed class RecorderRouteDefinition : IApiRouteDefinition
{
    public static RecorderRouteDefinition Instance { get; } = new();

    private RecorderRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var precondition = serverBuilder.UseInPlayableScenePrecondition();

        var outputRecorder = context.OutputRecorder;

        serverBuilder.MapGet(
            "recorder/settings",
            () => outputRecorder.Settings is { } sceneSettings ? Ok(sceneSettings) : NotFound()
        );

        serverBuilder.MapPut(
            "recorder/settings",
            (RecorderSettings newSettings) =>
            {
                if (outputRecorder.IsRecording)
                {
                    return ServiceUnavailable();
                }

                outputRecorder.Settings = newSettings;

                return Ok();
            }
        );

        serverBuilder.MapGet("recorder/is_able_to_record", () => outputRecorder.IsAbleToRecord);

        serverBuilder.MapGet(
            "recorder/frames_recorded",
            () =>
                outputRecorder.IsAbleToRecord
                    ? Ok(outputRecorder.FramesRecorded)
                    : ServiceUnavailable()
        );

        serverBuilder.MapGet(
            "recorder/frames_recorded_async",
            () =>
                outputRecorder.IsAbleToRecord
                    ? Ok(FramesRecordedCoroutine(outputRecorder))
                    : ServiceUnavailable()
        );

        serverBuilder.MapGet("recorder/enabled", () => outputRecorder.enabled);

        serverBuilder.MapPut(
            "recorder/enabled",
            (bool shouldRecord) =>
            {
                if ((shouldRecord, outputRecorder.IsAbleToRecord) is (true, false))
                {
                    return ServiceUnavailable();
                }

                if (shouldRecord != outputRecorder.IsRecording)
                {
                    outputRecorder.enabled = shouldRecord;
                }

                return Ok();
            }
        );
    }

    private IEnumerator FramesRecordedCoroutine(OutputRecorder outputRecorder)
    {
        while (true)
        {
            yield return $"{outputRecorder.FramesRecorded}\n";

            if (outputRecorder.IsRecording is false)
            {
                break;
            }

            yield return null;
        }
    }
}
