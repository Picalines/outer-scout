namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

internal sealed class FuncRequestHandler : RequestHandler
{
    private readonly Func<Request, Response> _HandlerFunc;

    public FuncRequestHandler(Route route, Func<Request, Response> handlerFunc)
        : base(route)
    {
        _HandlerFunc = handlerFunc;
    }

    protected override Response HandleInternal(Request request)
    {
        return _HandlerFunc.Invoke(request);
    }
}
