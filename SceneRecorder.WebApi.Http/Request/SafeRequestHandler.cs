using System.Reflection;
using Newtonsoft.Json;
using SceneRecorder.Infrastructure.Validation;
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
            var response = _requestHandler.Handle(request);

            response.ThrowIfNull();

            return response;
        }
        catch (Exception exception)
        {
            int depth = 0;

            while (
                exception is TargetInvocationException { InnerException: var innerException }
                && ++depth < 100
            )
            {
                exception = innerException;
            }

            if (exception is JsonSerializationException or JsonReaderException)
            {
                return ResponseFabric.BadRequest(exception.Message);
            }

            return ResponseFabric.InternalServerError(
                $"{exception.GetType()}: {exception.Message}\n{exception.StackTrace}"
            );
        }
    }
}
