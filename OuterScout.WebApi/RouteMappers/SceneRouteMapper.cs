using OuterScout.Application.Animation;
using OuterScout.Application.Recording;
using OuterScout.Application.SceneCameras;
using OuterScout.Domain;
using OuterScout.WebApi.Extensions;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Services;
using OWML.Common;

namespace OuterScout.WebApi.RouteMappers;

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
        IModConsole modConsole,
        ApiResourceRepository resources
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

        resources.DisposeResources<IAnimator>();
        resources.DisposeResources<ISceneCamera>();
        resources.DisposeResources<ApiOwnedGameObject>();

        resources.GlobalContainer.DisposeResource<SceneRecorder.Builder>();

        var sceneRecorderBuilder = new SceneRecorder.Builder();
        resources.GlobalContainer.AddResource(nameof(SceneRecorder), sceneRecorderBuilder);

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

    private static IResponse StartRecording(ApiResourceRepository resources)
    {
        if (
            resources.GlobalContainer.GetResource<SceneRecorder.Builder>()
            is not { } sceneRecorderBuilder
        )
        {
            return ServiceUnavailable();
        }

        sceneRecorderBuilder.WithScenePatch(
            () => { },
            () => resources.GlobalContainer.DisposeResource<SceneRecorder>()
        );

        resources.GlobalContainer.AddResource(
            nameof(SceneRecorder),
            sceneRecorderBuilder.StartRecording()
        );

        return Ok();
    }

    private static IResponse GetRecordingStatus(ApiResourceRepository resources)
    {
        if (
            resources.GlobalContainer.GetResource<SceneRecorder.Builder>()
            is not { } sceneRecorderBuilder
        )
        {
            return ServiceUnavailable();
        }

        var sceneRecorder = resources.GlobalContainer.GetResource<SceneRecorder>();

        return Ok(
            new
            {
                InProgress = sceneRecorder is not null,
                CurrentFrame = sceneRecorder is { CurrentFrame: var currentFrame }
                    ? currentFrame
                    : sceneRecorderBuilder.FrameRange.Start
            }
        );
    }
}
