using System.ComponentModel;
using System.Reflection;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.Http.Response;
using SceneRecorder.WebApi.Http.Routing;

namespace SceneRecorder.WebApi.Http;

internal sealed class FuncRequestHandler : RequestHandler
{
    private readonly Func<Request, IResponse> _HandlerFunc;

    private FuncRequestHandler(Route route, Func<Request, IResponse> handlerFunc)
        : base(route)
    {
        _HandlerFunc = handlerFunc;
    }

    protected override IResponse HandleInternal(Request request)
    {
        return _HandlerFunc.Invoke(request);
    }

    public static FuncRequestHandler Create(Route route, Delegate handler) =>
        new(route, BindHandler(route, handler));

    private static Func<Request, IResponse> BindHandler(Route route, Delegate handler)
    {
        var handlerMethod = handler.Method;
        var handlerTarget = handler.Target;
        var handlerParameters = handlerMethod.GetParameters();
        var handlerParametersCount = handlerParameters.Length;

        var returnsVoid = handlerMethod.ReturnType == typeof(void);

        var routeParameters = new HashSet<string>(route.Parameters);

        var handlerArgumentBinders = handlerParameters
            .Select<ParameterInfo, Func<Request, object>>(parameter =>
            {
                var parameterType = parameter.ParameterType;

                if (parameterType == typeof(Request))
                {
                    return request => request;
                }

                var parameterName = parameter.Name;

                if (parameterType.IsPrimitive || parameterType == typeof(string))
                {
                    var typeConverter = TypeDescriptor.GetConverter(parameterType);

                    return routeParameters.Contains(parameterName)
                        ? (
                            request =>
                                request.RouteParameters.TryGetValue(
                                    parameterName,
                                    out var routeValue
                                )
                                    ? typeConverter.ConvertFromString(routeValue)
                                    : ResponseFabric.BadRequest(
                                        $"missing route {parameterType.Name} parameter '${parameterName}'"
                                    )
                        )
                        : (
                            request =>
                                request.QueryParameters.TryGetValue(
                                    parameterName,
                                    out var queryValue
                                )
                                    ? typeConverter.ConvertFromString(queryValue)
                                    : ResponseFabric.BadRequest(
                                        $"missing query {parameterType.Name} parameter '${parameterName}'"
                                    )
                        );
                }

                var jsonBodyMethod =
                    typeof(Request)
                        .GetMethod(nameof(Request.JsonBody))
                        .MakeGenericMethod(parameterType)
                    ?? throw new InvalidOperationException(
                        $"{nameof(Request)}.{nameof(Request.JsonBody)}<{parameterType.FullName}> not found"
                    );

                return request => jsonBodyMethod.Invoke(request, Array.Empty<object>());
            })
            .ToArray();

        return request =>
        {
            var handlerArguments = new object[handlerParametersCount];

            foreach (var (index, binder) in handlerArgumentBinders.Indexed())
            {
                var argument = binder(request);

                if (argument is IResponse errorResponse)
                {
                    return errorResponse;
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
}
