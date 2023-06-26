using System.Net;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

public interface IResponse
{
    public HttpStatusCode StatusCode { get; }

    public string ContentType { get; }
}
