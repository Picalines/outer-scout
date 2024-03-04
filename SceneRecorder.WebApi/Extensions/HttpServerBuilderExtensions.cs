using SceneRecorder.Application.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.Extensions;

using SceneRecorder.Application.Recording;
using SceneRecorder.WebApi.Services;
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
                return resources.GlobalContainer.GetResource<SceneRecorder>() is not null
                    ? ServiceUnavailable(new { Error = "not available while recording" })
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
