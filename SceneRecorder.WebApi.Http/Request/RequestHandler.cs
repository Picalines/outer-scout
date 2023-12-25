using System.Reflection;
using SceneRecorder.WebApi.Http.Response;
using SceneRecorder.WebApi.Http.Routing;

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
            int depth = 0;
            while (
                exception is TargetInvocationException { InnerException: var innerException }
                && depth++ < 100
            )
            {
                exception = innerException;
            }

            return ResponseFabric.InternalServerError(
                $"{exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}"
            );
        }
    }
}
