namespace SceneRecorder.WebApi.Http;

internal abstract class RequestHandler
{
    public Route Route { get; }

    public RequestHandler(Route route)
    {
        Route = route;
    }

    protected abstract IResponse HandleInternal(Request request);

    public IResponse Handle(Request request)
    {
        try
        {
            return HandleInternal(request);
        }
        catch (Exception exception)
        {
            return ResponseFabric.InternalServerError(
                $"{exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}"
            );
        }
    }
}
