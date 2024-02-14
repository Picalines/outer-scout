using SceneRecorder.Application.Animation;
using SceneRecorder.Application.SceneCameras;
using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteMappers;

using OWML.Common;
using SceneRecorder.Application.Recording;
using static ResponseFabric;

internal sealed class SceneRouteMapper : IRouteMapper
{
    public static SceneRouteMapper Instance { get; } = new();

    private SceneRouteMapper() { }

    private sealed class CreateSceneRequest
    {
        public required int StartFrame { get; init; }

        public required int EndFrame { get; init; }

        public required int FrameRate { get; init; }

        public required bool HidePlayerModel { get; init; }
    }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        {
            using (serverBuilder.WithNotRecordingFilter())
            {
                serverBuilder.MapPost("scene", CreateScene);

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

    private static IResponse CreateScene(
        [FromBody] CreateSceneRequest request,
        ResettableLazy<SceneRecorder.Builder> lazySceneRecorderBuilder,
        IModConsole modConsole
    )
    {
        if (request.StartFrame > request.EndFrame)
        {
            return BadRequest("invalid frame range");
        }

        if (request.FrameRate < 1)
        {
            return BadRequest("invalid frame rate");
        }

        lazySceneRecorderBuilder.Reset();

        SceneResource.Find<IAnimator>().ForEach(resource => resource.Dispose());
        SceneResource.Find<ISceneCamera>().ForEach(resource => resource.Dispose());

        var sceneRecorderBuilder = lazySceneRecorderBuilder.Value;

        sceneRecorderBuilder
            .WithCaptureFrameRate(request.FrameRate)
            .WithFrameRange(IntRange.FromValues(request.StartFrame, request.EndFrame))
            .WithProgressLoggedToConsole(modConsole)
            .WithTimeScaleRestored()
            .WithInvinciblePlayer()
            .WithAllInputDevicesDisabled()
            .WithDisplayRenderingDisabled()
            .WithPauseMenuDisabled()
            .WithDisabledQuantumMoon();

        if (request.HidePlayerModel)
        {
            sceneRecorderBuilder.WithHiddenPlayerModel();
        }

        return Ok();
    }

    private static IResponse StartRecording(SceneRecorder.Builder sceneRecorderBuilder)
    {
        var gameObject = new GameObject(nameof(SceneRecorder));

        sceneRecorderBuilder.WithScenePatch(
            new(() => { }, () => UnityEngine.Object.Destroy(gameObject))
        );

        var sceneRecorder = sceneRecorderBuilder.StartRecording();

        gameObject.AddResource(sceneRecorder);

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
