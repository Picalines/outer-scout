using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Http;
using Picalines.OuterWilds.SceneRecorder.Json;

namespace Picalines.OuterWilds.SceneRecorder.WebInterop;

internal sealed class WebApiServer : IDisposable
{
    private readonly IModConsole _ModConsole;

    private readonly SceneRecorderMod _SceneRecorderMod;

    private HttpServer? _HttpServer;

    public WebApiServer(
        IModConsole modConsole,
        SceneRecorderSettings settings,
        SceneRecorderMod sceneRecorderMod)
    {
        _ModConsole = modConsole;
        _SceneRecorderMod = sceneRecorderMod;

        var httpServerBuilder = new HttpServerBuilder(settings.WebApiUrl);

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

            if ((shouldRecord, _SceneRecorderMod.IsAbleToRecord) is (true, false))
            {
                return Response.ServiceUnavailable("Unable to record scene");
            }

            if (shouldRecord == _SceneRecorderMod.IsRecording)
            {
                return Response.NotModified<string>();
            }

            bool success = (shouldRecord, _SceneRecorderMod.IsRecording) switch
            {
                (true, false) => _SceneRecorderMod.TryStartRecording(),
                (false, true) => success = _SceneRecorderMod.TryStopRecording(),
                _ => throw new InvalidProgramException(),
            };

            return success
                ? Response.Ok<string>()
                : Response.ServiceUnavailable($"Failed to {(shouldRecord ? "start" : "stop")} recording");
        });
    }
}
