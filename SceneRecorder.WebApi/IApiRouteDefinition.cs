using Picalines.OuterWilds.SceneRecorder.Recording.Recorders;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;

namespace Picalines.OuterWilds.SceneRecorder.WebApi;

internal interface IApiRouteDefinition
{
    public interface IContext
    {
        public OutputRecorder OutputRecorder { get; }
    }

    public void MapRoutes(HttpServerBuilder serverBuilder, IContext context);
}
