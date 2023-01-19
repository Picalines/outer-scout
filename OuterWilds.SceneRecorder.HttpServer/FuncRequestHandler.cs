using Newtonsoft.Json;

namespace OuterWilds.SceneRecorder.HttpServer;

internal sealed class FuncRequestHandler<T> : RequestHandler<T>
{
    private readonly Func<RequestHandlerContext, Response<T>> _HandlerFunc;

    public FuncRequestHandler(
        JsonSerializerSettings jsonSerializerSettings,
        Route route,
        Func<RequestHandlerContext, Response<T>> handlerFunc)
        : base(jsonSerializerSettings, route)
    {
        _HandlerFunc = handlerFunc;
    }

    protected override Response<T> Handle(RequestHandlerContext context)
    {
        return _HandlerFunc.Invoke(context);
    }
}
