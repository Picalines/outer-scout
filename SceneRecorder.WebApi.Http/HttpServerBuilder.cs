namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

public sealed class HttpServerBuilder
{
    internal sealed class PreconditionHandler : IDisposable
    {
        public Func<Request, Response?> OptionalRequestHandler { get; }

        private readonly Stack<PreconditionHandler> _PreconditionStack;

        private bool _Disposed = false;

        public PreconditionHandler(Stack<PreconditionHandler> preconditionStack, Func<Request, Response?> requestHandler)
        {
            OptionalRequestHandler = requestHandler;
            _PreconditionStack = preconditionStack;

            _PreconditionStack.Push(this);
        }

        public void Dispose()
        {
            if (_Disposed)
            {
                return;
            }

            if (_PreconditionStack.TryPop(out var precondition) is false
                || precondition != this)
            {
                throw new InvalidOperationException($"invalid use of {nameof(HttpServerBuilder)}.{nameof(HttpServerBuilder.UsePrecondition)}");
            }

            _Disposed = true;
        }
    }

    private readonly string _BaseUrl;

    private readonly List<RequestHandler> _RequestHandlers = new();

    private readonly Stack<PreconditionHandler> _PreconditionHandlerStack = new();

    private PreconditionHandler[] _CurrentPreconditionHandlerQueue = Array.Empty<PreconditionHandler>();

    public HttpServerBuilder(string baseUrl)
    {
        _BaseUrl = baseUrl;
    }

    public void MapGet(string route, Func<Request, Response> handler) => Map(HttpMethod.GET, route, handler);

    public void MapPost(string route, Func<Request, Response> handler) => Map(HttpMethod.POST, route, handler);

    public void MapPut(string route, Func<Request, Response> handler) => Map(HttpMethod.PUT, route, handler);

    public void MapDelete(string route, Func<Request, Response> handler) => Map(HttpMethod.DELETE, route, handler);

    public void MapPatch(string route, Func<Request, Response> handler) => Map(HttpMethod.PATCH, route, handler);

    public IDisposable UsePrecondition(Func<Request, Response?> optionalRequestHandler)
    {
        var preconditionHandler = new PreconditionHandler(_PreconditionHandlerStack, optionalRequestHandler);

        _CurrentPreconditionHandlerQueue = _PreconditionHandlerStack.Reverse().ToArray();

        return preconditionHandler;
    }

    public void Build(HttpServer httpServer)
    {
        httpServer.Configure(_BaseUrl, _RequestHandlers.ToArray());
    }

    private void Map(HttpMethod httpMethod, string path, Func<Request, Response> handler)
    {
        var route = new Route(httpMethod, Route.ParsePathString(path));

        var preconditionHandlers = _CurrentPreconditionHandlerQueue;

        _RequestHandlers.Add(new FuncRequestHandler(route, request =>
        {
            foreach (var preconditionHandler in preconditionHandlers)
            {
                var optionalResponse = preconditionHandler.OptionalRequestHandler.Invoke(request);
                if (optionalResponse is not null)
                {
                    return optionalResponse;
                }
            }

            return handler(request);
        }));
    }
}
