using System.Net;

namespace OuterScout.WebApi.Http.Response;

public sealed record StringResponse : IResponse
{
    public HttpStatusCode StatusCode { get; }

    public string ContentType { get; } = "text/plain";

    public string Content { get; }

    public StringResponse(HttpStatusCode statusCode, string content)
    {
        StatusCode = statusCode;
        Content = content;
    }

    void IResponse.InternalOnly() { }
}
