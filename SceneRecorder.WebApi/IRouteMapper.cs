using OuterScout.WebApi.Http;

namespace OuterScout.WebApi;

internal interface IRouteMapper
{
    public void MapRoutes(HttpServer.Builder serverBuilder);
}
