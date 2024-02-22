namespace SceneRecorder.WebApi.Http.Response;

public sealed class ResponseException : Exception
{
    public IResponse Response { get; }

    public ResponseException(IResponse response)
    {
        Response = response;
    }
}
