using System.Net;

namespace OuterScout.WebApi.Http.Response;

public sealed class StreamResponse : IResponse
{
    public HttpStatusCode StatusCode { get; }

    public string ContentType { get; init; } = "text/plain";

    public Stream Stream { get; }

    public StreamResponse(HttpStatusCode statusCode, Stream stream)
    {
        StatusCode = statusCode;
        Stream = stream;
    }

    void IResponse.InternalOnly() { }
}
