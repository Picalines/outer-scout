using System.ComponentModel;

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
        var routeParamConverters = new Dictionary<string, TypeConverter>();
        var queryParamConverters = new Dictionary<string, TypeConverter>();

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
            var typeConverter = TypeDescriptor.GetConverter(type);
            (isRouteParameter ? routeParamConverters : queryParamConverters)[param] = typeConverter;
        }

        return request =>
        {
            if (queryParamConverters.Count > request.QueryParameters.Count)
            {
                var unexpectedParameterName = request.QueryParameters.Keys
                    .Except(queryParamConverters.Keys)
                    .First();

                return ResponseFabric.BadRequest($"unexpected query parameter '{unexpectedParameterName}'");
            }

            if (queryParamConverters.Count < request.QueryParameters.Count)
            {
                var missingParameterName = queryParamConverters.Keys
                    .Except(request.QueryParameters.Keys)
                    .First();

                return ResponseFabric.BadRequest($"missing query parameter '{missingParameterName}'");
            }

            var handlerParameters = new object[handlerParametersCount + 1];

            handlerParameters[0] = request;

            foreach (var (name, strValue) in request.RouteParameters)
            {
                var value = routeParamConverters[name].ConvertFromInvariantString(strValue);
                handlerParameters[handlerParameterIndexes[name] + 1] = value;
            }

            foreach (var (name, strValue) in request.QueryParameters)
            {
                var value = queryParamConverters[name].ConvertFromInvariantString(strValue);
                handlerParameters[handlerParameterIndexes[name] + 1] = value;
            }

            return (Response)handlerFunc.Method.Invoke(handlerFunc.Target, handlerParameters);
        };
    }
}
