using System.Collections;
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

        var argumentBinders = handlerParameters
            .Indexed()
            .Select<(int, ParameterInfo), IBinder>(pair =>
            {
                var (parameterIndex, parameter) = pair;
                var parameterType = parameter.ParameterType;
                var parameterName = parameter.Name;

                if (parameterType.IsPrimitive || parameterType == typeof(string))
                {
                    return route.ParameterIndexes.TryGetValue(parameterName, out var pathIndex)
                        ? new PathBinder(pathIndex, parameterType)
                        : new QueryBinder(parameterName, parameterType);
                }

                if (parameter.GetCustomAttribute<FromBodyAttribute>() is { })
                {
                    return new JsonBodyBinder(services, parameterType);
                }

                return new ServiceBinder(services, parameterType, parameter.IsNullable());
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
                string @string => ResponseFabric.Ok(@string),
                IEnumerator coroutine => ResponseFabric.Ok(coroutine),
                var json => ResponseFabric.Ok(json),
            };
        };
    }

    private sealed class PathBinder(int pathIndex, Type paramType) : IBinder
    {
        private readonly TypeConverter _typeConverter = TypeDescriptor.GetConverter(paramType);

        public object? Bind(Request request)
        {
            request.Path.Count.Throw().IfLessThan(pathIndex);
            return _typeConverter.ConvertFromString(request.Path[pathIndex]);
        }
    }

    private sealed class QueryBinder(string paramName, Type paramType) : IBinder
    {
        private readonly TypeConverter _typeConverter = TypeDescriptor.GetConverter(paramType);

        private readonly IResponse _badRequest = ResponseFabric.BadRequest(
            $"missing query parameter '{paramName}' ({paramType.Name})"
        );

        public object? Bind(Request request)
        {
            return request.QueryParameters.TryGetValue(paramName, out var paramValue)
                ? _typeConverter.ConvertFromString(paramValue)
                : _badRequest;
        }
    }

    private sealed class ServiceBinder(ServiceContainer services, Type paramType, bool isNullable)
        : IBinder
    {
        private readonly IResponse _internalError = ResponseFabric.InternalServerError(
            $"missing service of type {paramType}"
        );

        public object? Bind(Request request)
        {
            var instance = services.ResolveOrNull(paramType);

            if ((instance, isNullable) is (null, false))
            {
                return _internalError;
            }

            return instance;
        }
    }

    private sealed class JsonBodyBinder(ServiceContainer services, Type paramType) : IBinder
    {
        public object? Bind(Request request)
        {
            var jsonSerializer = services.Resolve<JsonSerializer>();

            using var jsonReader = new JsonTextReader(request.BodyReader);
            return jsonSerializer.Deserialize(jsonReader, paramType);
        }
    }
}
