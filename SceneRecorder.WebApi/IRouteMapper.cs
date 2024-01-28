using SceneRecorder.Application.Recording;
using SceneRecorder.WebApi.Http;

namespace SceneRecorder.WebApi;

internal interface IRouteMapper
{
    public interface IContext
    {
        public OutputRecorder OutputRecorder { get; }
    }

    public void MapRoutes(HttpServerBuilder serverBuilder, IContext context);
}
