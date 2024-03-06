using System.Net;

namespace OuterScout.WebApi.Http.Response;

public sealed record EmptyResponse : IResponse
{
    public HttpStatusCode StatusCode { get; }

    public string ContentType { get; } = "text/plain";

    private static readonly Dictionary<HttpStatusCode, EmptyResponse> _instances = [];

    private EmptyResponse(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }

    public static EmptyResponse WithStatusCode(HttpStatusCode statusCode)
    {
        if (_instances.TryGetValue(statusCode, out var response) is false)
        {
            _instances.Add(statusCode, response = new EmptyResponse(statusCode));
        }

        return response;
    }

    void IResponse.InternalOnly() { }
}
