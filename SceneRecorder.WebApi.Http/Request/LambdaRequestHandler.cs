using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;
using SceneRecorder.Infrastructure.DependencyInjection;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Infrastructure.Validation;
using SceneRecorder.WebApi.Http.Response;
using SceneRecorder.WebApi.Http.Routing;

namespace SceneRecorder.WebApi.Http;

internal sealed class LambdaRequestHandler : IRequestHandler
{
    private readonly Func<Request, IResponse> _handler;

    private LambdaRequestHandler(Func<Request, IResponse> handler)
    {
        _handler = handler;
    }

    public IResponse Handle(Request request)
    {
        return _handler.Invoke(request);
    }

    public static LambdaRequestHandler Create(
        ServiceContainer services,
        Route route,
        Delegate handler
    )
    {
        return new(BindDelegate(services, route, handler));
    }

    private interface IBinder
    {
        public object? Bind(Request request);
    }

    private static Func<Request, IResponse> BindDelegate(
        ServiceContainer services,
        Route route,
        Delegate handler
    )
    {
        var handlerMethod = handler.Method;
        var handlerTarget = handler.Target;
        var handlerParameters = handlerMethod.GetParameters();
        var handlerParametersCount = handlerParameters.Length;

        var returnsVoid = handlerMethod.ReturnType == typeof(void);

        var routeParameters = new HashSet<string>(route.Parameters);

        var argumentBinders = handlerParameters
            .Select<ParameterInfo, IBinder>(parameter =>
            {
                var parameterType = parameter.ParameterType;

                var parameterName = parameter.Name;

                if (parameterType.IsPrimitive || parameterType == typeof(string))
                {
                    return new UrlBinder(
                        parameterName,
                        parameterType,
                        routeParameters.Contains(parameterName)
                            ? UrlBinder.SourceType.Route
                            : UrlBinder.SourceType.Query
                    );
                }

                if (parameter.GetCustomAttribute<FromBodyAttribute>() is { })
                {
                    return new JsonBodyBinder(services, parameterType);
                }

                return new ServiceBinder(services, parameterType);
            })
            .ToArray();

        return request =>
        {
            var handlerArguments = new object?[handlerParametersCount];

            foreach (var (index, binder) in argumentBinders.Indexed())
            {
                var argument = binder.Bind(request);

                if (argument is IResponse earlyResponse)
                {
                    return earlyResponse;
                }

                handlerArguments[index] = argument;
            }

            return handlerMethod.Invoke(handlerTarget, handlerArguments) switch
            {
                _ when returnsVoid => ResponseFabric.Ok(),
                IResponse response => response,
                var value => ResponseFabric.Ok(value),
            };
        };
    }

    private sealed class UrlBinder(string paramName, Type paramType, UrlBinder.SourceType source)
        : IBinder
    {
        public enum SourceType
        {
            Route,
            Query,
        }

        private readonly TypeConverter _typeConverter = TypeDescriptor.GetConverter(paramType);

        private readonly string _badRequestMessage = source switch
        {
            SourceType.Route => $"missing route parameter '{paramName}' ({paramType.Name})",
            SourceType.Query => $"missing query parameter '{paramName}' ({paramType.Name})",
            _ => throw new NotImplementedException(),
        };

        public object? Bind(Request request)
        {
            return request.RouteParameters.TryGetValue(paramName, out var paramValue)
                ? _typeConverter.ConvertFromString(paramValue)
                : ResponseFabric.BadRequest(_badRequestMessage);
        }
    }

    private sealed class ServiceBinder(ServiceContainer services, Type paramType) : IBinder
    {
        public object? Bind(Request request)
        {
            return services.Resolve(paramType)
                ?? throw new InvalidOperationException(
                    $"failed to resolve service of type {paramType}"
                );
        }
    }

    private sealed class JsonBodyBinder(ServiceContainer services, Type paramType) : IBinder
    {
        public object? Bind(Request request)
        {
            var jsonSerializer = services.Resolve<JsonSerializer>();
            jsonSerializer.ThrowIfNull();

            using var jsonReader = new JsonTextReader(request.BodyReader);
            return jsonSerializer.Deserialize(jsonReader, paramType);
        }
    }
}
