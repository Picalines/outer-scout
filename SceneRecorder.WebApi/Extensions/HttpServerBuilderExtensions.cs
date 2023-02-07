using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Extensions;

internal static class HttpServerBuilderExtensions
{
    public static IDisposable UseInGameScenePrecondition(this HttpServerBuilder serverBuilder)
    {
        return serverBuilder.UsePrecondition(request =>
        {
            return LocatorExtensions.IsInSolarSystemScene() is false
                ? ResponseFabric.ServiceUnavailable("not in game scene")
                : null;
        });
    }
}
