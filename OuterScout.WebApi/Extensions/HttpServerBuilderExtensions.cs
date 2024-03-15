using OuterScout.Application.Extensions;
using OuterScout.Application.Recording;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Services;

namespace OuterScout.WebApi.Extensions;

using static ResponseFabric;

internal static class HttpServerBuilderExtensions
{
    public static IDisposable WithPlayableSceneFilter(this HttpServer.Builder serverBuilder)
    {
        return serverBuilder.WithFilter(() =>
        {
            return LocatorExtensions.IsInPlayableScene() is false
                ? ServiceUnavailable(new { Error = "not in playable scene" })
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
                    ? ServiceUnavailable(new { Error = "not available during recording" })
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
                    ? ServiceUnavailable(new { Error = "not available, create a scene first" })
                    : null;
            }
        );
    }
}
