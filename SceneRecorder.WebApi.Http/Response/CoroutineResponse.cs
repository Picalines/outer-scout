using System.Collections;
using System.Net;

namespace SceneRecorder.WebApi.Http.Response;

public sealed class CoroutineResponse : IResponse
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
}