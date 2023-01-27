using Newtonsoft.Json;
using System.Net;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

public sealed class Response
{
    public HttpStatusCode StatusCode { get; }

    public string ContentType { get; }

    public string Content { get; }

    private Response(HttpStatusCode statusCode, string contentType, string content)
    {
        StatusCode = statusCode;
        ContentType = contentType;
        Content = content;
    }

    public void ToHttpListenerResponse(HttpListenerResponse response)
    {
        response.StatusCode = (int)StatusCode;
        response.ContentType = ContentType;
        response.ContentLength64 = Content.Length;
        using var contentWriter = new StreamWriter(response.OutputStream);
        contentWriter.Write(Content);
    }

    public static Response FromString(HttpStatusCode httpStatusCode, string value)
    {
        return new(httpStatusCode, "text/plain", value);
    }

    public static Response Empty(HttpStatusCode httpStatusCode)
    {
        return FromString(httpStatusCode, "");
    }

    public static Response FromJson<T>(HttpStatusCode httpStatusCode, T value)
    {
        return new(httpStatusCode, "application/json", JsonConvert.SerializeObject(value));
    }
}
