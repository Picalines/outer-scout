using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

internal sealed class HttpServerBuilder
{
    private readonly string _BaseUrl;

    private readonly List<RequestHandler> _RequestHandlers = new();

    private bool _IsBuilt = false;

    public HttpServerBuilder(string baseUrl)
    {
        _BaseUrl = baseUrl;
    }

    public void MapGet(string route, Func<Request, Response> handler) => Map(HttpMethod.GET, route, handler);

    public void MapPost(string route, Func<Request, Response> handler) => Map(HttpMethod.POST, route, handler);

    public void MapPut(string route, Func<Request, Response> handler) => Map(HttpMethod.PUT, route, handler);

    public void MapDelete(string route, Func<Request, Response> handler) => Map(HttpMethod.DELETE, route, handler);

    public void MapPatch(string route, Func<Request, Response> handler) => Map(HttpMethod.PATCH, route, handler);

    public HttpServer Build(GameObject gameObject)
    {
        AssertNotBuilt();

        _IsBuilt = true;

        var httpServer = gameObject.AddComponent<HttpServer>();
        httpServer.Configure(_BaseUrl, _RequestHandlers.ToArray());
        return httpServer;
    }

    private void Map(HttpMethod httpMethod, string path, Func<Request, Response> handler)
    {
        AssertNotBuilt();

        var route = new Route(httpMethod, Route.ParsePathString(path));
        _RequestHandlers.Add(new FuncRequestHandler(route, handler));
    }

    private void AssertNotBuilt()
    {
        if (_IsBuilt)
        {
            throw new InvalidOperationException();
        }
    }
}
