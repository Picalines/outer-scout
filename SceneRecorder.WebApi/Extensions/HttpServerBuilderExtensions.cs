using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Extensions;

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
