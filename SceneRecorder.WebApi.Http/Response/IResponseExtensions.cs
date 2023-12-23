using System.Net;

namespace SceneRecorder.WebApi.Http.Response;

public static class IResponseExtensions
{
    public static void SetHeaders(this IResponse response, HttpListenerResponse listenerResponse)
    {
        listenerResponse.StatusCode = (int)response.StatusCode;

        listenerResponse.ContentType =
            response.ContentType
            + (response.ContentType.Contains("charset") is false ? "; charset=utf-8" : "");
    }
}
