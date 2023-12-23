using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.Http;

namespace SceneRecorder.WebApi.Extensions;

internal static class HttpServerBuilderExtensions
{
    public static IDisposable UseInPlayableScenePrecondition(this HttpServerBuilder serverBuilder)
    {
        return serverBuilder.UsePrecondition(request =>
        {
            return LocatorExtensions.IsInPlayableScene() is false
                ? ResponseFabric.ServiceUnavailable("not in playable scene")
                : null;
        });
    }
}
