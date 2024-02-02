using System.Reflection;
using Newtonsoft.Json;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.Http;

internal sealed class SafeRequestHandler : IRequestHandler
{
    private readonly IRequestHandler _requestHandler;

    public SafeRequestHandler(IRequestHandler requestHandler)
    {
        _requestHandler = requestHandler;
    }

    public IResponse Handle(Request request)
    {
        try
        {
            return _requestHandler.Handle(request);
        }
        catch (JsonSerializationException jsonException)
        {
            return ResponseFabric.BadRequest(jsonException.Message);
        }
        catch (Exception internalException)
        {
            int depth = 0;

            while (
                internalException
                    is TargetInvocationException { InnerException: var innerException }
                && ++depth < 100
            )
            {
                internalException = innerException;
            }

            return ResponseFabric.InternalServerError(
                $"{internalException.GetType()}: {internalException.Message}\n{internalException.StackTrace}"
            );
        }
    }
}
