using System.Collections;
using System.Net;

namespace OuterScout.WebApi.Http.Response;

public sealed record CoroutineResponse : IResponse
{
    public HttpStatusCode StatusCode { get; }

    public string ContentType { get; }

    internal IEnumerator Coroutine { get; }

    internal CoroutineResponse(HttpStatusCode statusCode, string contentType, IEnumerator coroutine)
    {
        StatusCode = statusCode;
        ContentType = contentType;
        Coroutine = coroutine;
    }

    void IResponse.InternalOnly() { }
}
