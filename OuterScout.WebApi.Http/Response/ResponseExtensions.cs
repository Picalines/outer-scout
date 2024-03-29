namespace OuterScout.WebApi.Http.Response;

public static class ResponseExtensions
{
    public static ResponseException ToException(this IResponse response)
    {
        return new ResponseException(response);
    }

    public static bool IsSuccessful(this IResponse response)
    {
        return (int)response.StatusCode is >= 200 and < 300;
    }
}
