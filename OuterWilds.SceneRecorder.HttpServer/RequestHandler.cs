using Newtonsoft.Json;
using System.Net;

namespace OuterWilds.SceneRecorder.HttpServer;

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
            response = Response.InternalServerError<T>(default!);

#if DEBUG
            listenerResponse.StatusCode = (int)response.StatusCode;
            listenerResponse.ContentType = "text/plain";
            WriteContent(listenerResponse, exception.ToString());
#endif
        }

        var jsonValue = JsonConvert.SerializeObject(response.Value, JsonSerializerSettings);

        listenerResponse.StatusCode = (int)response.StatusCode;
        listenerResponse.ContentType = "application/json";

        WriteContent(listenerResponse, jsonValue);
    }

    private void WriteContent(HttpListenerResponse listenerResponse, string content)
    {
        listenerResponse.ContentLength64 = content.Length;
        using var writer = new StreamWriter(listenerResponse.OutputStream);
        writer.Write(content);
    }
}
