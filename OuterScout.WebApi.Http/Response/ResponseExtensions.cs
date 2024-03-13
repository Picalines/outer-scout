namespace OuterScout.WebApi.Http.Response;

public static class ResponseExtensions
{
    public static ResponseException ToException(this IResponse response)
    {
        return new ResponseException(response);
    }
}
