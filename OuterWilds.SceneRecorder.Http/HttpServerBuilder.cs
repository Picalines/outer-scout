using Newtonsoft.Json;

namespace Picalines.OuterWilds.SceneRecorder.Http;

public sealed class HttpServerBuilder
{
    private readonly string _BaseUrl;

    private readonly JsonSerializerSettings _JsonSerializerSettings;

    private readonly List<IRequestHandler> _RequestHandlers = new();

    private bool _IsBuilt = false;

    public HttpServerBuilder(string baseUrl, JsonSerializerSettings? jsonSerializerSettings = null)
    {
        _BaseUrl = baseUrl;
        _JsonSerializerSettings = jsonSerializerSettings ?? new();
    }

    public void MapGet<T>(string route, Func<RequestHandlerContext, Response<T>> handler) => Map(HttpMethod.GET, route, handler);

    public void MapPost<T>(string route, Func<RequestHandlerContext, Response<T>> handler) => Map(HttpMethod.POST, route, handler);

    public void MapPut<T>(string route, Func<RequestHandlerContext, Response<T>> handler) => Map(HttpMethod.PUT, route, handler);

    public void MapDelete<T>(string route, Func<RequestHandlerContext, Response<T>> handler) => Map(HttpMethod.DELETE, route, handler);

    public void MapPatch<T>(string route, Func<RequestHandlerContext, Response<T>> handler) => Map(HttpMethod.PATCH, route, handler);

    public HttpServer Build()
    {
        AssertNotBuilt();

        _IsBuilt = true;
        return new HttpServer(_BaseUrl, _RequestHandlers.ToArray());
    }

    private void Map<T>(HttpMethod httpMethod, string path, Func<RequestHandlerContext, Response<T>> handler)
    {
        AssertNotBuilt();

        var route = new Route(httpMethod, Route.ParsePathString(path));
        _RequestHandlers.Add(new FuncRequestHandler<T>(_JsonSerializerSettings, route, handler));
    }

    private void AssertNotBuilt()
    {
        if (_IsBuilt)
        {
            throw new InvalidOperationException();
        }
    }
}
