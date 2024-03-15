using OuterScout.Application.Extensions;
using OuterScout.Application.Recording;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Services;

namespace OuterScout.WebApi.Extensions;

internal static class HttpServerBuilderExtensions
{
    public static IDisposable WithPlayableSceneFilter(this HttpServer.Builder serverBuilder)
    {
        return serverBuilder.WithFilter(() =>
        {
            return LocatorExtensions.IsInPlayableScene() is false
                ? CommonResponse.NotInPlayableScene
                : null;
        });
    }

    public static IDisposable WithNotRecordingFilter(this HttpServer.Builder serverBuilder)
    {
        return serverBuilder.WithFilter(
            (ApiResourceRepository resources) =>
            {
                return
                    resources.GlobalContainer.GetResource<SceneRecorder>() is { IsRecording: true }
                    ? CommonResponse.RecordingIsInProgress
                    : null;
            }
        );
    }

    public static IDisposable WithSceneCreatedFilter(this HttpServer.Builder serverBuilder)
    {
        return serverBuilder.WithFilter(
            (ApiResourceRepository resources) =>
            {
                return resources.GlobalContainer.GetResource<SceneRecorder.Builder>() is null
                    ? CommonResponse.SceneIsNotCreated
                    : null;
            }
        );
    }
}
