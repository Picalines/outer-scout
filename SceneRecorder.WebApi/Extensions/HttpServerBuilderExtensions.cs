using SceneRecorder.Application.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.Extensions;

internal static class HttpServerBuilderExtensions
{
    public static IDisposable WithPlayableSceneFilter(this HttpServer.Builder serverBuilder)
    {
        return serverBuilder.WithFilter(request =>
        {
            return LocatorExtensions.IsInPlayableScene() is false
                ? ResponseFabric.ServiceUnavailable(new { Error = "not in playable scene" })
                : null;
        });
    }
}
