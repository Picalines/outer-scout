using System.Collections;
using SceneRecorder.WebApi.DTOs;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class RecorderRouteMapper : IRouteMapper
{
    public static RecorderRouteMapper Instance { get; } = new();

    private RecorderRouteMapper() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IRouteMapper.IContext context)
    {
        using var precondition = serverBuilder.UseInPlayableScenePrecondition();

        var outputRecorder = context.OutputRecorder;

        serverBuilder.MapGet(
            "recorder/settings",
            () => outputRecorder.Settings is { } sceneSettings ? Ok(sceneSettings) : NotFound()
        );

        serverBuilder.MapPut(
            "recorder/settings",
            (SceneSettingsDTO newSettings) =>
            {
                if (outputRecorder.IsRecording)
                {
                    return ServiceUnavailable();
                }

                outputRecorder.Settings = newSettings;

                return Ok();
            }
        );

        serverBuilder.MapGet(
            "recorder/status",
            () =>
                new RecorderStatusResponse
                {
                    Enabled = outputRecorder.enabled,
                    IsAbleToRecord = outputRecorder.IsAbleToRecord,
                    FramesRecorded = outputRecorder.FramesRecorded,
                }
        );

        serverBuilder.MapGet(
            "recorder/frames-recorded-async",
            () =>
                outputRecorder.IsAbleToRecord
                    ? Ok(FramesRecordedCoroutine(outputRecorder))
                    : ServiceUnavailable()
        );

        serverBuilder.MapPut(
            "recorder/enabled",
            (bool value) =>
            {
                if ((value, outputRecorder.IsAbleToRecord) is (true, false))
                {
                    return ServiceUnavailable();
                }

                if (value != outputRecorder.IsRecording)
                {
                    outputRecorder.enabled = value;
                }

                return Ok();
            }
        );
    }

    private static IEnumerator FramesRecordedCoroutine(OutputRecorder outputRecorder)
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
