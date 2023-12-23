using System.Net;

namespace SceneRecorder.WebApi.Http.Response;

public interface IResponse
{
    public HttpStatusCode StatusCode { get; }

    public string ContentType { get; }
}
