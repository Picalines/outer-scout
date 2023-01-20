using Newtonsoft.Json;
using System.Net;

namespace Picalines.OuterWilds.SceneRecorder.Http;

public sealed record RequestHandlerContext(Request Request);

internal abstract class RequestHandler<T> : IRequestHandler
{
    public JsonSerializerSettings JsonSerializerSettings { get; }

    public Route Route { get; }

    public RequestHandler(JsonSerializerSettings jsonSerializerSettings, Route route)
    {
        JsonSerializerSettings = jsonSerializerSettings;
        Route = route;
    }

    protected abstract Response<T> Handle(RequestHandlerContext context);

    public void BuildResponse(Request request, HttpListenerResponse listenerResponse)
    {
        Response<T> response;

        var context = new RequestHandlerContext(request);

        try
        {
            response = Handle(context);
        }
        catch (Exception exception)
        {
            response = Response.InternalServerError<T>();

#if DEBUG
            listenerResponse.StatusCode = (int)response.StatusCode;
            WriteContent(listenerResponse, "text/plain", exception.ToString());
            return;
#endif
        }

        listenerResponse.StatusCode = (int)response.StatusCode;

        if (response.HasValue is false)
        {
            WriteContent(listenerResponse, "text/plain", "");
            return;
        }

        var jsonValue = JsonConvert.SerializeObject(response.Value, JsonSerializerSettings);

        WriteContent(listenerResponse, "application/json", jsonValue);
    }

    private void WriteContent(HttpListenerResponse listenerResponse, string contentType, string content)
    {
        listenerResponse.ContentType = contentType;
        listenerResponse.ContentLength64 = content.Length;
        using var writer = new StreamWriter(listenerResponse.OutputStream);
        writer.Write(content);
    }
}
