using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Http;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders;

namespace Picalines.OuterWilds.SceneRecorder.WebInterop;

internal sealed class WebApiServer : IDisposable
{
    public string Url { get; }

    private readonly OutputRecorder _OutputRecorder;

    private HttpServer? _HttpServer;

    public WebApiServer(
        IModConfig modConfig,
        OutputRecorder outputRecorder)
    {
        _OutputRecorder = outputRecorder;

        Url = $"http://localhost:{modConfig.GetSettingsValue<int>("web_api_port")}/";
        var httpServerBuilder = new HttpServerBuilder(Url);

        MapRoutes(httpServerBuilder);

        _HttpServer = httpServerBuilder.Build();

        _HttpServer.StartListening();
    }

    public void Dispose()
    {
        if (_HttpServer is null)
        {
            return;
        }

        _HttpServer.StopListening();
        _HttpServer = null;
    }

    private void MapRoutes(HttpServerBuilder serverBuilder)
    {
        serverBuilder.MapPut("recorder&{enabled:bool}", context =>
        {
            var shouldRecord = context.Request.GetQueryParameter<bool>("enabled");

            if ((shouldRecord, _OutputRecorder.IsAbleToRecord) is (true, false))
            {
                return Response.ServiceUnavailable("Unable to record scene");
            }

            if (shouldRecord == _OutputRecorder.IsRecording)
            {
                return Response.NotModified<string>();
            }

            _OutputRecorder.enabled = shouldRecord;

            return Response.Ok<string>();
        });
    }
}
