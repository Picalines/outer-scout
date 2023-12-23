using System.Net;

namespace SceneRecorder.WebApi.Http;

public interface IResponse
{
    public HttpStatusCode StatusCode { get; }

    public string ContentType { get; }
}
