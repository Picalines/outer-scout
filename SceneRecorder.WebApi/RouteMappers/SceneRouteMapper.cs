using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.DTOs;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

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

        SceneResource.FindInstances<object>().ForEach(resource => resource.Dispose());

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
}
