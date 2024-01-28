using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.Http;

public interface IRequestHandler
{
    public IResponse Handle(Request request);
}
