using SceneRecorder.Application.Animation;
using SceneRecorder.Application.SceneCameras;
using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.Components;
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
            serverBuilder.MapPost("scene", CreateNewScene);
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

        lazyBuilder.Reset();
        DisposeResources();

        var sceneRecorderBuilder = lazyBuilder.Value;

        sceneRecorderBuilder.WithFrameRange(
            IntRange.FromValues(settings.Frames.Start, settings.Frames.End)
        );

        if (settings.HidePlayerModel)
        {
            sceneRecorderBuilder.WithHiddenPlayerModel();
        }

        return Ok();
    }

    private static void DisposeResources()
    {
        GameObject
            .FindObjectsOfType<SceneResource<IAnimator>>()
            .ForEach(resource => resource.Dispose());

        GameObject
            .FindObjectsOfType<SceneResource<ISceneCamera>>()
            .ForEach(sceneCameraResource =>
            {
                sceneCameraResource.Dispose();
                UnityEngine.Object.Destroy(sceneCameraResource.gameObject);
            });
    }
}
