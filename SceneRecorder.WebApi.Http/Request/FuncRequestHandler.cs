using System.ComponentModel;
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

    public static FuncRequestHandler CreateUnchecked(
        Route route,
        Func<Request, IResponse> handlerFunc
    ) => new(route, handlerFunc);

    public static FuncRequestHandler Create(Route route, Func<Request, IResponse> handlerFunc) =>
        new(route, CreateHandler(route, handlerFunc));

    public static FuncRequestHandler Create<T1>(
        Route route,
        Func<Request, T1, IResponse> handlerFunc
    ) => new(route, CreateHandler(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2>(
        Route route,
        Func<Request, T1, T2, IResponse> handlerFunc
    ) => new(route, CreateHandler(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2, T3>(
        Route route,
        Func<Request, T1, T2, T3, IResponse> handlerFunc
    ) => new(route, CreateHandler(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2, T3, T4>(
        Route route,
        Func<Request, T1, T2, T3, T4, IResponse> handlerFunc
    ) => new(route, CreateHandler(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2, T3, T4, T5>(
        Route route,
        Func<Request, T1, T2, T3, T4, T5, IResponse> handlerFunc
    ) => new(route, CreateHandler(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2, T3, T4, T5, T6>(
        Route route,
        Func<Request, T1, T2, T3, T4, T5, T6, IResponse> handlerFunc
    ) => new(route, CreateHandler(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2, T3, T4, T5, T6, T7>(
        Route route,
        Func<Request, T1, T2, T3, T4, T5, T6, T7, IResponse> handlerFunc
    ) => new(route, CreateHandler(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2, T3, T4, T5, T6, T7, T8>(
        Route route,
        Func<Request, T1, T2, T3, T4, T5, T6, T7, T8, IResponse> handlerFunc
    ) => new(route, CreateHandler(route, handlerFunc));

    private static Func<Request, IResponse> CreateHandler(Route route, Delegate handlerFunc)
    {
        var handlerParameters = handlerFunc
            .Method.GetParameters()
            .Skip(1)
            .Select(
                (param, index) =>
                    new
                    {
                        Index = index,
                        param.Name,
                        TypeName = param.ParameterType.Name,
                        TypeConverter = TypeDescriptor.GetConverter(param.ParameterType),
                    }
            )
            .ToDictionary(param => param.Name, param => param);

        var queryParameterNames = new HashSet<string>(
            handlerParameters
                .Values.Select(param => param.Name)
                .Except(
                    route
                        .Segments.Where(segment => segment.Type is Route.SegmentType.Parameter)
                        .Select(segment => segment.Value)
                )
        );

        var queryParametersCount = queryParameterNames.Count;

        return request =>
        {
            if (request.QueryParameters.Count < queryParametersCount)
            {
                var missingParameterName = queryParameterNames
                    .Except(request.QueryParameters.Keys)
                    .First();

                return ResponseFabric.BadRequest(
                    $"missing query parameter '{missingParameterName}'"
                );
            }

            if (request.QueryParameters.Count >= queryParametersCount)
            {
                var unexpectedParameterName = request
                    .QueryParameters.Keys.Except(queryParameterNames)
                    .FirstOrDefault();

                if (unexpectedParameterName is not null)
                {
                    return ResponseFabric.BadRequest(
                        $"unexpected query parameter '{unexpectedParameterName}'"
                    );
                }
            }

            var handlerArguments = new object[handlerParameters.Count + 1];

            handlerArguments[0] = request;

            foreach (
                var (name, strValue) in request.RouteParameters.Concat(request.QueryParameters)
            )
            {
                if (handlerParameters.TryGetValue(name, out var handlerParameter) is false)
                {
                    return ResponseFabric.BadRequest($"unexpected route parameter '{name}'");
                }

                try
                {
                    handlerArguments[1 + handlerParameter.Index] =
                        handlerParameter.TypeConverter.ConvertFromInvariantString(strValue);
                }
                catch (NotSupportedException)
                {
                    return ResponseFabric.BadRequest(
                        $"{handlerParameter.TypeName} expected in '{name}' parameter"
                    );
                }
            }

            return (IResponse)handlerFunc.Method.Invoke(handlerFunc.Target, handlerArguments);
        };
    }
}
