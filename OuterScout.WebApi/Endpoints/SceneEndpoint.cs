using OuterScout.Application.Animation;
using OuterScout.Application.Extensions;
using OuterScout.Application.Recording;
using OuterScout.Shared.Collections;
using OuterScout.Shared.DependencyInjection;
using OuterScout.Shared.Extensions;
using OuterScout.WebApi.DTOs;
using OuterScout.WebApi.Extensions;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Services;
using OWML.Common;
using UnityEngine;

namespace OuterScout.WebApi.Endpoints;

using static ResponseFabric;

internal sealed class SceneEndpoint : IRouteMapper, IServiceConfiguration
{
    public const string OriginResource = "scene.origin";

    public static SceneEndpoint Instance { get; } = new();

    private SceneEndpoint() { }

    private sealed class CreateSceneRequest
    {
        public required bool HidePlayerModel { get; init; }

        public required TransformDto Origin { get; init; }
    }

    private sealed class StartRecordingRequest
    {
        public required int FrameRate { get; init; }

        public required int StartFrame { get; init; }

        public required int EndFrame { get; init; }
    }

    void IRouteMapper.MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        {
            using (serverBuilder.WithNotRecordingFilter())
            {
                serverBuilder.MapPost("scene", PostScene);
                serverBuilder.MapDelete("scene", DeleteScene);

                using (serverBuilder.WithSceneCreatedFilter())
                {
                    serverBuilder.MapPost("scene/recording", PostRecording);
                }
            }

            using (serverBuilder.WithSceneCreatedFilter())
            {
                serverBuilder.MapGet("scene/recording/status", GetRecordingStatus);
            }
        }
    }

    void IServiceConfiguration.RegisterServices(ServiceContainer.Builder services)
    {
        services
            .Register<RecordingProgressGUI>()
            .InstantiatePerUnityScene()
            .InstantiateAsComponentWithServices();
    }

    public static Transform? GetOriginOrNull(GameObjectRepository gameObjects)
    {
        return gameObjects.GetOwnOrNull(OriginResource)?.transform;
    }

    private static IResponse PostScene(
        [FromBody] CreateSceneRequest request,
        ApiResourceRepository resources,
        GameObjectRepository gameObjects
    )
    {
        if (
            DeleteScene(resources, gameObjects) is { } deleteResponse
            && deleteResponse.IsSuccessful() is false
        )
        {
            return deleteResponse;
        }

        if (request.Origin.Parent is null)
        {
            return CommonResponse.InvalidBodyField(
                "origin.parent",
                "scene.origin must have a parent GameObject"
            );
        }

        if (gameObjects.FindOrNull(request.Origin.Parent) is not { transform: var originParent })
        {
            return CommonResponse.GameObjectNotFound(request.Origin.Parent);
        }

        var sceneRecorderBuilder = new SceneRecorder.Builder();
        resources.GlobalContainer.AddResource(nameof(SceneRecorder), sceneRecorderBuilder);

        var originObject = new GameObject($"{nameof(OuterScout)}.{OriginResource}");
        originObject.transform.parent = originParent;
        originObject.transform.ResetLocal();
        request.Origin.ApplyLocal(originObject.transform);
        gameObjects.AddOwned(OriginResource, originObject);

        sceneRecorderBuilder
            .WithProgressLoggedToConsole(Singleton<IModConsole>.Instance)
            .WithTimeScaleRestored()
            .WithInvinciblePlayer()
            .WithAllInputDevicesDisabled()
            .WithHudDisabled()
            .WithDisplayRenderingDisabled()
            .WithPauseMenuDisabled()
            .WithDisabledQuantumMoon();

        if (request.HidePlayerModel)
        {
            sceneRecorderBuilder.WithHiddenPlayerModel();
        }
        else if (Locator.GetActiveCamera() == Locator.GetPlayerCamera())
        {
            sceneRecorderBuilder.WithPlayerHeadVisible();
        }

        return Created();
    }

    private static IResponse DeleteScene(
        ApiResourceRepository resources,
        GameObjectRepository gameObjects
    )
    {
        resources.DisposeResources<PropertyAnimator>();
        resources.DisposeResources<IRecorder.IBuilder>();
        gameObjects.DestroyOwnObjects();

        resources.GlobalContainer.DisposeResource<SceneRecorder.Builder>();

        return Ok();
    }

    private static IResponse PostRecording(
        [FromBody] StartRecordingRequest request,
        ApiResourceRepository resources,
        RecordingProgressGUI progressGUI
    )
    {
        if (request.StartFrame > request.EndFrame)
        {
            return CommonResponse.InvalidBodyField("startFrame");
        }

        if (request.FrameRate < 1)
        {
            return CommonResponse.InvalidBodyField("frameRate");
        }

        var sceneRecorderBuilder =
            resources.GlobalContainer.GetRequiredResource<SceneRecorder.Builder>();

        KeyframesEndpoint
            .GetOrderedPropertyAnimators(resources)
            .ForEach(animator => sceneRecorderBuilder.WithAnimator(animator));

        var numberOfRecorders = resources
            .GetResources<IRecorder.IBuilder>()
            .Tap(recorder => sceneRecorderBuilder.WithRecorder(recorder))
            .Count();

        if (numberOfRecorders is 0)
        {
            return BadRequest(new Problem("noRecorders") { Title = "No recorders on the scene" });
        }

        resources.GlobalContainer.DisposeResources<SceneRecorder>();

        sceneRecorderBuilder.WithScenePatch(
            () => progressGUI.enabled = true,
            () => progressGUI.enabled = false
        );

        resources.GlobalContainer.AddResource(
            nameof(SceneRecorder),
            sceneRecorderBuilder.StartRecording(
                new SceneRecorder.RecordingParameters()
                {
                    FrameRange = IntRange.FromValues(request.StartFrame, request.EndFrame),
                    FrameRate = request.FrameRate,
                }
            )
        );

        return Created();
    }

    private static IResponse GetRecordingStatus(ApiResourceRepository resources)
    {
        if (resources.GlobalContainer.GetResource<SceneRecorder>() is not { } sceneRecorder)
        {
            return ServiceUnavailable();
        }

        return Ok(
            new
            {
                InProgress = sceneRecorder.IsRecording,
                StartFrame = sceneRecorder.FrameRange.Start,
                EndFrame = sceneRecorder.FrameRange.End,
                CurrentFrame = sceneRecorder.CurrentFrame,
                FramesRecorded = sceneRecorder.FramesRecorded,
            }
        );
    }
}
