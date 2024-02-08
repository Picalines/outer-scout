using System.Collections;
using System.Net;

namespace SceneRecorder.WebApi.Http.Response;

internal sealed class CoroutineResponse : IResponse
{
    public HttpStatusCode StatusCode { get; }

    public string ContentType { get; }

    public IEnumerator Coroutine { get; }

    internal CoroutineResponse(HttpStatusCode statusCode, string contentType, IEnumerator coroutine)
    {
        StatusCode = statusCode;
        ContentType = contentType;
        Coroutine = coroutine;
    }

    void IResponse.InternalOnly() { }
}
