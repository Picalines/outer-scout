using System.Net;

namespace SceneRecorder.WebApi.Http.Response;

public sealed record JsonResponse : IResponse
{
    public HttpStatusCode StatusCode { get; }

    public string ContentType { get; } = "application/json";

    public object? Value { get; }

    public JsonResponse(HttpStatusCode statusCode, object? value)
    {
        StatusCode = statusCode;
        Value = value;
    }

    void IResponse.InternalOnly() { }
}
