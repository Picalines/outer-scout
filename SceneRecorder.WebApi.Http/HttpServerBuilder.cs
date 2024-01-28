using SceneRecorder.WebApi.Http.Response;
using SceneRecorder.WebApi.Http.Routing;

namespace SceneRecorder.WebApi.Http;

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

    public void MapGet(string path, Delegate handler) => Map(HttpMethod.Get, path, handler);

    public void MapPost(string path, Delegate handler) => Map(HttpMethod.Post, path, handler);

    public void MapPut(string path, Delegate handler) => Map(HttpMethod.Put, path, handler);

    public void MapDelete(string path, Delegate handler) => Map(HttpMethod.Delete, path, handler);

    public IDisposable UsePrecondition(Func<Request, IResponse?> optionalRequestHandler)
    {
        return new PreconditionHandler(_PreconditionHandlerStack, optionalRequestHandler);
    }

    public void Build(HttpServer httpServer)
    {
        httpServer.Configure(_BaseUrl, _RequestHandlers.ToArray());
    }

    private void Map(HttpMethod method, string path, Delegate handlerFunc)
    {
        var route = RouteFromString(method, path);
        var handler = LambdaRequestHandler.Create(route, handlerFunc);

        var preconditionHandlers = _PreconditionHandlerStack.Reverse().ToArray();

        var wrappedHandler = LambdaRequestHandler.Create(
            route,
            (Request request) =>
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
        );

        _RequestHandlers.Add(wrappedHandler);
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
