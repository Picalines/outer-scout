using SceneRecorder.Application.Animation;
using SceneRecorder.Application.SceneCameras;
using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.DTOs;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteMappers;

using SceneRecorder.Application.Recording;
using static ResponseFabric;

internal sealed class SceneRouteMapper : IRouteMapper
{
    public static SceneRouteMapper Instance { get; } = new();

    private SceneRouteMapper() { }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        {
            using (serverBuilder.WithNotRecordingFilter())
            {
                serverBuilder.MapPost("scene", CreateNewScene);

                using (serverBuilder.WithSceneCreatedFilter())
                {
                    serverBuilder.MapPost("scene/recording", StartRecording);
                }
            }

            using (serverBuilder.WithSceneCreatedFilter())
            {
                serverBuilder.MapGet("scene/recording/status", GetRecordingStatus);
            }
        }
    }

    private static IResponse CreateNewScene(
        [FromBody] SceneSettingsDTO settings,
        ResettableLazy<SceneRecorder.Builder> lazyBuilder
    )
    {
        if (settings.Frames.Start > settings.Frames.End)
        {
            return BadRequest("invalid frame range");
        }

        if (settings.Frames.Rate < 1)
        {
            return BadRequest("invalid frame rate");
        }

        lazyBuilder.Reset();

        SceneResource.Find<IAnimator>().ForEach(resource => resource.Dispose());
        SceneResource.Find<ISceneCamera>().ForEach(resource => resource.Dispose());

        var sceneRecorderBuilder = lazyBuilder.Value;

        sceneRecorderBuilder
            .WithCaptureFrameRate(settings.Frames.Rate)
            .WithFrameRange(IntRange.FromValues(settings.Frames.Start, settings.Frames.End));

        if (settings.HidePlayerModel)
        {
            sceneRecorderBuilder.WithHiddenPlayerModel();
        }

        return Ok();
    }

    private static IResponse StartRecording(SceneRecorder.Builder sceneRecorderBuilder)
    {
        var sceneRecorder = sceneRecorderBuilder.StartRecording();

        new GameObject($"{nameof(SceneRecorder)}").AddResource(sceneRecorder);

        return Ok();
    }

    private static IResponse GetRecordingStatus(SceneRecorder.Builder sceneRecorderBuilder)
    {
        var sceneRecorder = SceneResource.Find<SceneRecorder>().SingleOrDefault();

        return Ok(
            new
            {
                InProgress = sceneRecorder is not null,
                CurrentFrame = sceneRecorder is { Value.CurrentFrame: var currentFrame }
                    ? currentFrame
                    : sceneRecorderBuilder.FrameRange.Start
            }
        );
    }
}
