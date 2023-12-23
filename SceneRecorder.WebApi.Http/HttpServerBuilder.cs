namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

public sealed class HttpServerBuilder
{
    internal sealed class PreconditionHandler : IDisposable
    {
        public Func<Request, IResponse?> OptionalRequestHandler { get; }

        private readonly Stack<PreconditionHandler> _PreconditionStack;

        private bool _Disposed = false;

        public PreconditionHandler(
            Stack<PreconditionHandler> preconditionStack,
            Func<Request, IResponse?> requestHandler
        )
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

            if (_PreconditionStack.TryPop(out var precondition) is false || precondition != this)
            {
                throw new InvalidOperationException(
                    $"invalid use of {nameof(HttpServerBuilder)}.{nameof(UsePrecondition)}"
                );
            }

            _Disposed = true;
        }
    }

    private readonly string _BaseUrl;

    private readonly List<RequestHandler> _RequestHandlers = new();

    private readonly Stack<PreconditionHandler> _PreconditionHandlerStack = new();

    public HttpServerBuilder(string baseUrl)
    {
        _BaseUrl = baseUrl;
    }

    public void Map(HttpMethod httpMethod, string path, Func<Request, IResponse> handler) =>
        Map(FuncRequestHandler.Create(RouteFromString(httpMethod, path), handler));

    public void Map<T1>(HttpMethod httpMethod, string path, Func<Request, T1, IResponse> handler) =>
        Map(FuncRequestHandler.Create(RouteFromString(httpMethod, path), handler));

    public void Map<T1, T2>(
        HttpMethod httpMethod,
        string path,
        Func<Request, T1, T2, IResponse> handler
    ) => Map(FuncRequestHandler.Create(RouteFromString(httpMethod, path), handler));

    public void Map<T1, T2, T3>(
        HttpMethod httpMethod,
        string path,
        Func<Request, T1, T2, T3, IResponse> handler
    ) => Map(FuncRequestHandler.Create(RouteFromString(httpMethod, path), handler));

    public void Map<T1, T2, T3, T4>(
        HttpMethod httpMethod,
        string path,
        Func<Request, T1, T2, T3, T4, IResponse> handler
    ) => Map(FuncRequestHandler.Create(RouteFromString(httpMethod, path), handler));

    public void Map<T1, T2, T3, T4, T5>(
        HttpMethod httpMethod,
        string path,
        Func<Request, T1, T2, T3, T4, T5, IResponse> handler
    ) => Map(FuncRequestHandler.Create(RouteFromString(httpMethod, path), handler));

    public void Map<T1, T2, T3, T4, T5, T6>(
        HttpMethod httpMethod,
        string path,
        Func<Request, T1, T2, T3, T4, T5, T6, IResponse> handler
    ) => Map(FuncRequestHandler.Create(RouteFromString(httpMethod, path), handler));

    public void Map<T1, T2, T3, T4, T5, T6, T7>(
        HttpMethod httpMethod,
        string path,
        Func<Request, T1, T2, T3, T4, T5, T6, T7, IResponse> handler
    ) => Map(FuncRequestHandler.Create(RouteFromString(httpMethod, path), handler));

    public void Map<T1, T2, T3, T4, T5, T6, T7, T8>(
        HttpMethod httpMethod,
        string path,
        Func<Request, T1, T2, T3, T4, T5, T6, T7, T8, IResponse> handler
    ) => Map(FuncRequestHandler.Create(RouteFromString(httpMethod, path), handler));

    public IDisposable UsePrecondition(Func<Request, IResponse?> optionalRequestHandler)
    {
        return new PreconditionHandler(_PreconditionHandlerStack, optionalRequestHandler);
    }

    public void Build(HttpServer httpServer)
    {
        httpServer.Configure(_BaseUrl, _RequestHandlers.ToArray());
    }

    private void Map(RequestHandler handler)
    {
        var preconditionHandlers = _PreconditionHandlerStack.Reverse().ToArray();

        _RequestHandlers.Add(
            FuncRequestHandler.CreateUnchecked(
                handler.Route,
                request =>
                {
                    foreach (var preconditionHandler in preconditionHandlers)
                    {
                        var optionalResponse = preconditionHandler.OptionalRequestHandler.Invoke(
                            request
                        );
                        if (optionalResponse is not null)
                        {
                            return optionalResponse;
                        }
                    }

                    return handler.Handle(request);
                }
            )
        );
    }

    private static Route RouteFromString(HttpMethod httpMethod, string path)
    {
        if (Route.TryFromString(httpMethod, path, out var route) is false)
        {
            throw new InvalidOperationException($"invalid {httpMethod} route {path}");
        }

        return route;
    }
}
