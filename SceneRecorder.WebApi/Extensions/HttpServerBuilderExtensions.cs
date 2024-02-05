using SceneRecorder.Application.Extensions;
using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Validation;
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
                ? null
                : ServiceUnavailable(new { Error = "not available while recording" });
        });
    }

    public static IDisposable WithSceneCreatedFilter(this HttpServer.Builder serverBuilder)
    {
        return serverBuilder.WithFilter(
            (request, services) =>
            {
                var sceneRecorderBuilder = services.Resolve<
                    ResettableLazy<SceneRecorder.Builder>
                >();

                sceneRecorderBuilder.ThrowIfNull();

                return sceneRecorderBuilder.IsValueCreated
                    ? null
                    : ServiceUnavailable(new { Error = "not available, create a scene first" });
            }
        );
    }
}
