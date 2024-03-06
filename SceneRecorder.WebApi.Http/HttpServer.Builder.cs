using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using OuterScout.Infrastructure.DependencyInjection;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Http.Routing;

namespace OuterScout.WebApi.Http;

using RequestHandler = Func<IServiceContainer, IResponse>;

public sealed partial class HttpServer
{
    public sealed class Builder
    {
        private readonly string _baseUrl;

        private readonly ServiceContainer.Builder _serviceBuilder;

        private readonly Router<RequestHandler>.Builder _routerBuilder = new();

        private readonly Stack<WithFilterOnStack> _filterStack = new();

        private bool _built = false;

        public Builder(string baseUrl, ServiceContainer.Builder services)
        {
            _baseUrl = baseUrl;
            _serviceBuilder = services;

            services.RegisterIfMissing<JsonSerializer>();

            services
                .Register<UrlParameterBinder>()
                .As<IParameterBinder>()
                .InScope(RequestScopeName);

            services.Register<RequestBodyBinder>().As<IParameterBinder>().InScope(RequestScopeName);
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

        public IDisposable WithFilter(Delegate filter)
        {
            return new WithFilterOnStack(_filterStack, filter);
        }

        public HttpServer Build()
        {
            if (_built)
            {
                throw new InvalidOperationException($"{nameof(Build)} called twice");
            }

            _built = true;

            HttpServer httpServer = null!;

            _serviceBuilder
                .Register<Route>()
                .InScope(RequestScopeName)
                .InstantiateBy(() => httpServer._currentRoute!);

            _serviceBuilder
                .Register<Request>()
                .InScope(RequestScopeName)
                .InstantiateBy(() => httpServer._currentRequest!);

            httpServer = new HttpServer(_baseUrl, _serviceBuilder.Build(), _routerBuilder.Build());

            return httpServer;
        }

        private void Map(HttpMethod method, string path, Delegate handlerFunc)
        {
            var route = RouteFromString(method, path);

            var handler = handlerFunc.BindByContainer();

            var filters = _filterStack.Select(f => f.Filter.BindByContainer()).Reverse().ToArray();

            var filteredHandler = (IServiceContainer services) =>
            {
                foreach (var filter in filters)
                {
                    if (filter(services) is { } result)
                    {
                        return result;
                    }
                }

                return handler(services);
            };

            _routerBuilder.WithRoute(route, services => GetResponse(services, filteredHandler));
        }

        private static IResponse GetResponse(
            IServiceContainer services,
            Func<IServiceContainer, object?> handler
        )
        {
            try
            {
                var result = handler.Invoke(services);

                return result switch
                {
                    null => ResponseFabric.Ok(),
                    IResponse response => response,
                    string @string => ResponseFabric.Ok(@string),
                    IEnumerator coroutine => ResponseFabric.Ok(coroutine),
                    _ => ResponseFabric.Ok(result),
                };
            }
            catch (Exception exception)
            {
                int depth = 0;

                while (
                    exception is TargetInvocationException { InnerException: var innerException }
                    && ++depth < 100
                )
                {
                    exception = innerException;
                }

                if (exception is ResponseException { Response: var response })
                {
                    return response;
                }

                if (exception is JsonSerializationException or JsonReaderException)
                {
                    return ResponseFabric.BadRequest(exception.Message);
                }

                return ResponseFabric.InternalServerError(
                    $"{exception.GetType()}: {exception.Message}\n{exception.StackTrace}"
                );
            }
        }

        private static Route RouteFromString(HttpMethod httpMethod, string path)
        {
            if (Route.FromString(httpMethod, path) is not { } route)
            {
                throw new InvalidOperationException($"invalid {httpMethod} route {path}");
            }

            return route;
        }

        private sealed class WithFilterOnStack : IDisposable
        {
            public Delegate Filter { get; }

            private readonly Stack<WithFilterOnStack> _filterStack;

            private bool _disposed = false;

            public WithFilterOnStack(Stack<WithFilterOnStack> filterStack, Delegate requestHandler)
            {
                Filter = requestHandler;
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
