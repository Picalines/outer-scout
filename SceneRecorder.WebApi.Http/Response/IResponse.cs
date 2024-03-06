using System.Net;

namespace OuterScout.WebApi.Http.Response;

public interface IResponse
{
    public HttpStatusCode StatusCode { get; }

    public string ContentType { get; }

    internal void InternalOnly();
}
