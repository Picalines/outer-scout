using SceneRecorder.Recording.Recorders;
using SceneRecorder.WebApi.Http;

namespace SceneRecorder.WebApi;

internal interface IApiRouteDefinition
{
    public interface IContext
    {
        public OutputRecorder OutputRecorder { get; }
    }

    public void MapRoutes(HttpServerBuilder serverBuilder, IContext context);
}
