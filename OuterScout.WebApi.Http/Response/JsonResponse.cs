using System.Net;

namespace OuterScout.WebApi.Http.Response;

public sealed record JsonResponse : IResponse
{
    public HttpStatusCode StatusCode { get; }

    public string ContentType { get; }

    public object? Value { get; }

    public JsonResponse(HttpStatusCode statusCode, object? value)
    {
        StatusCode = statusCode;
        ContentType = value is Problem ? "application/problem+json" : "application/json";
        Value = value;
    }

    void IResponse.InternalOnly() { }
}
