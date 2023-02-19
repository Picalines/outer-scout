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

    internal void Send(HttpListenerResponse response)
    {
        response.StatusCode = (int)StatusCode;

        var contentType = ContentType;
        if (contentType.Contains("charset") is false)
        {
            contentType += "; charset=utf-8";
        }
        response.ContentType = contentType;

        response.ContentLength64 = Content.Length;
        using (var contentWriter = new StreamWriter(response.OutputStream))
        {
            contentWriter.Write(Content);
        }

        response.Close();
    }

    public static Response FromString(HttpStatusCode httpStatusCode, string value)
    {
        var contentType = (value.StartsWith("<!DOCTYPE") || value.StartsWith("<html>"))
            ? "text/html"
            : "text/plain";

        return new(httpStatusCode, contentType, value);
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
