using SceneRecorder.WebApi.Http;

namespace SceneRecorder.WebApi;

internal interface IRouteMapper
{
    public void MapRoutes(HttpServer.Builder serverBuilder);
}
