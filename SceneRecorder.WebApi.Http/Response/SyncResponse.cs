using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SceneRecorder.WebApi.Http.Response;

public sealed class SyncResponse : IResponse
{
    public HttpStatusCode StatusCode { get; }

    public string ContentType { get; }

    public string Content { get; }

    private static JsonSerializerSettings _JsonSettings =
        new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

    private SyncResponse(HttpStatusCode statusCode, string contentType, string content)
    {
        StatusCode = statusCode;
        ContentType = contentType;
        Content = content;
    }

    public static SyncResponse FromString(HttpStatusCode httpStatusCode, string content)
    {
        var contentType =
            (content.StartsWith("<!DOCTYPE") || content.StartsWith("<html>"))
                ? "text/html"
                : "text/plain";

        return new(httpStatusCode, contentType, content);
    }

    public static SyncResponse Empty(HttpStatusCode httpStatusCode)
    {
        return FromString(httpStatusCode, "");
    }

    public static SyncResponse FromJson<T>(HttpStatusCode httpStatusCode, T value)
    {
        return new(
            httpStatusCode,
            "application/json",
            JsonConvert.SerializeObject(value, _JsonSettings)
        );
    }

    internal void Send(HttpListenerResponse listenerResponse)
    {
        this.SetHeaders(listenerResponse);

        using (var contentWriter = new StreamWriter(listenerResponse.OutputStream))
        {
            contentWriter.Write(Content);
        }

        listenerResponse.Close();
    }
}
