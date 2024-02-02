using Newtonsoft.Json;
using SceneRecorder.Infrastructure.DependencyInjection;
using SceneRecorder.WebApi.Http.Response;
using SceneRecorder.WebApi.Http.Routing;

namespace SceneRecorder.WebApi.Http;

public sealed partial class HttpServer
{
    public sealed class Builder
    {
        private readonly string _baseUrl;
        private readonly ServiceContainer _services;
        private readonly Router.Builder _routerBuilder = new();
        private readonly Stack<RequestFiler> _filterStack = new();

        private bool _built = false;

        public Builder(string baseUrl, ServiceContainer services)
        {
            _baseUrl = baseUrl;
            _services = services;

            RegisterJsonServices();
        }

        public void MapGet(string path, Delegate handler)
        {
            Map(HttpMethod.Get, path, handler);
        }

        public void MapPost(string path, Delegate handler)
        {
            Map(HttpMethod.Post, path, handler);
        }

        public void MapPut(string path, Delegate handler)
        {
            Map(HttpMethod.Put, path, handler);
        }

        public void MapDelete(string path, Delegate handler)
        {
            Map(HttpMethod.Delete, path, handler);
        }

        public IDisposable WithFilter(Func<Request, IResponse?> filter)
        {
            return new RequestFiler(_filterStack, filter);
        }

        public HttpServer Build()
        {
            if (_built)
            {
                throw new InvalidOperationException($"{nameof(Build)} called twice");
            }

            _built = true;

            return new HttpServer(_baseUrl, _services, _routerBuilder.Build());
        }

        private void Map(HttpMethod method, string path, Delegate handlerFunc)
        {
            var route = RouteFromString(method, path);

            var handler = LambdaRequestHandler.Create(_services, route, handlerFunc);

            var filters = _filterStack.Reverse().ToArray();

            IRequestHandler wrappedHandler = LambdaRequestHandler.Create(
                _services,
                route,
                (Request request) =>
                {
                    foreach (var filter in filters)
                    {
                        if (filter.RequestHandler(request) is { } response)
                        {
                            return response;
                        }
                    }

                    return handler.Handle(request);
                }
            );

            wrappedHandler = new SafeRequestHandler(wrappedHandler);

            _routerBuilder.WithRoute(route, wrappedHandler);
        }

        private void RegisterJsonServices()
        {
            var jsonSerializer = new JsonSerializer()
            {
                MissingMemberHandling = MissingMemberHandling.Error,
            };

            _services.RegisterInstance(jsonSerializer);
        }

        private static Route RouteFromString(HttpMethod httpMethod, string path)
        {
            if (Route.FromString(httpMethod, path) is not { } route)
            {
                throw new InvalidOperationException($"invalid {httpMethod} route {path}");
            }

            return route;
        }

        private sealed class RequestFiler : IDisposable
        {
            public Func<Request, IResponse?> RequestHandler { get; }

            private readonly Stack<RequestFiler> _filterStack;

            private bool _disposed = false;

            public RequestFiler(
                Stack<RequestFiler> filterStack,
                Func<Request, IResponse?> requestHandler
            )
            {
                RequestHandler = requestHandler;
                _filterStack = filterStack;

                _filterStack.Push(this);
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                while (_filterStack.TryPop(out var filter) && filter != this) { }
            }
        }
    }
}
