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
                ? null
                : ServiceUnavailable(new { Error = "not available while recording" });
        });
    }
}
