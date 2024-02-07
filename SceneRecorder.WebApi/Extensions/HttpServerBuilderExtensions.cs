using SceneRecorder.Application.Extensions;
using SceneRecorder.Domain;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.Extensions;

using SceneRecorder.Application.Recording;
using static ResponseFabric;

internal static class HttpServerBuilderExtensions
{
    public static IDisposable WithPlayableSceneFilter(this HttpServer.Builder serverBuilder)
    {
        return serverBuilder.WithFilter(request =>
        {
            return LocatorExtensions.IsInPlayableScene() is false
                ? ServiceUnavailable(new { Error = "not in playable scene" })
                : null;
        });
    }

    public static IDisposable WithNotRecordingFilter(this HttpServer.Builder serverBuilder)
    {
        return serverBuilder.WithFilter(request =>
        {
            return SceneResource.Find<SceneRecorder>().Any()
                ? ServiceUnavailable(new { Error = "not available while recording" })
                : null;
        });
    }

    public static IDisposable WithSceneCreatedFilter(this HttpServer.Builder serverBuilder)
    {
        return serverBuilder.WithFilter(
            (request, services) =>
            {
                return
                    services.Resolve<ResettableLazy<SceneRecorder.Builder>>()
                        is { IsValueCreated: true }
                    ? ServiceUnavailable(new { Error = "not available, create a scene first" })
                    : null;
            }
        );
    }
}
