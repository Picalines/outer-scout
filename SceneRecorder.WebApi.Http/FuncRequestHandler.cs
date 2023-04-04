namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

internal sealed class FuncRequestHandler : RequestHandler
{
    private readonly Func<Request, Response> _HandlerFunc;

    private FuncRequestHandler(Route route, Func<Request, Response> handlerFunc)
        : base(route)
    {
        _HandlerFunc = handlerFunc;
    }

    protected override Response HandleInternal(Request request)
    {
        return _HandlerFunc.Invoke(request);
    }

    public static FuncRequestHandler Create(Route route, Func<Request, Response> handlerFunc)
    {
        return new FuncRequestHandler(route, handlerFunc);
    }

    public static FuncRequestHandler Create<T1>(Route route, Func<Request, T1, Response> handlerFunc)
        => Create(route, CreateHandlerWithParameters(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2>(Route route, Func<Request, T1, T2, Response> handlerFunc)
        => Create(route, CreateHandlerWithParameters(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2, T3>(Route route, Func<Request, T1, T2, T3, Response> handlerFunc)
        => Create(route, CreateHandlerWithParameters(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2, T3, T4>(Route route, Func<Request, T1, T2, T3, T4, Response> handlerFunc)
        => Create(route, CreateHandlerWithParameters(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2, T3, T4, T5>(Route route, Func<Request, T1, T2, T3, T4, T5, Response> handlerFunc)
        => Create(route, CreateHandlerWithParameters(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2, T3, T4, T5, T6>(Route route, Func<Request, T1, T2, T3, T4, T5, T6, Response> handlerFunc)
        => Create(route, CreateHandlerWithParameters(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2, T3, T4, T5, T6, T7>(Route route, Func<Request, T1, T2, T3, T4, T5, T6, T7, Response> handlerFunc)
        => Create(route, CreateHandlerWithParameters(route, handlerFunc));

    public static FuncRequestHandler Create<T1, T2, T3, T4, T5, T6, T7, T8>(Route route, Func<Request, T1, T2, T3, T4, T5, T6, T7, T8, Response> handlerFunc)
        => Create(route, CreateHandlerWithParameters(route, handlerFunc));

    private static Func<Request, Response> CreateHandlerWithParameters(Route route, Delegate handlerFunc)
    {
        var routeParameters = new Dictionary<string, Type>();
        var queryParameters = new Dictionary<string, Type>();

        var handlerParameters = handlerFunc.Method.GetParameters().Skip(1).ToArray();
        var handlerParametersCount = handlerParameters.Length;

        var handlerParameterTypes = handlerParameters
            .ToDictionary(param => param.Name, param => param.ParameterType);

        var handlerParameterIndexes = handlerParameters
            .Select((param, index) => new { param.Name, index })
            .ToDictionary(param => param.Name, param => param.index);

        var routeParameterNames = new HashSet<string>(route.Segments
            .Where(segment => segment.Type is Route.SegmentType.Parameter)
            .Select(segment => segment.Value));

        foreach (var (param, type) in handlerParameterTypes)
        {
            var isRouteParameter = routeParameterNames.Contains(param);
            (isRouteParameter ? routeParameters : queryParameters)[param] = type;
        }

        return request =>
        {
            var parameters = new object[handlerParametersCount];

            // TODO

            return (Response)handlerFunc.Method.Invoke(null, parameters);
        };
    }
}
