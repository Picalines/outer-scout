using OuterWilds.SceneRecorder.HttpServer;
using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Json;
using System.Diagnostics;
using System.Reflection;

namespace Picalines.OuterWilds.SceneRecorder.WebInterop;

internal sealed class WebUI : IDisposable
{
    private readonly IModConsole _ModConsole;

    private Process? _UIProcess;

    private HttpServer? _HttpServer;

    public WebUI(IModConsole modConsole, SceneRecorderSettings settings)
    {
        _ModConsole = modConsole;

        _UIProcess = Process.Start(new ProcessStartInfo()
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = "dotnet",
            WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "WebUI"),
            Arguments = $"OuterWilds.SceneRecorder.WebUI.dll --urls \"http://localhost:{settings.WebUIPort}\"",
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });

        _UIProcess.OutputDataReceived += OnUIProcessDataReceived;
        _UIProcess.ErrorDataReceived += OnUIProcessErrorReceived;
        _UIProcess.EnableRaisingEvents = true;
        _UIProcess.BeginOutputReadLine();

        var serverBuilder = new HttpServerBuilder($"http://localhost:{settings.WebUIPort + 1}/");

        serverBuilder.MapGet("", request =>
        {
            return Response.Ok(new { Message = "Hello, world!" });
        });

        serverBuilder.MapGet("api", request =>
        {
            return Response.Ok(new { Message = "Hello, API!" });
        });

        serverBuilder.MapGet("api/nums/{number:\\d+}", context =>
        {
            return Response.Ok(new { Value = context.Request.RouteParameters["number"] });
        });

        _HttpServer = serverBuilder.Build();
        _HttpServer.StartListening();
    }

    public void Dispose()
    {
        if (_UIProcess is not null)
        {
            if (_UIProcess.HasExited is false)
            {
                _UIProcess.Kill();
            }

            _UIProcess.Dispose();
            _UIProcess = null;
        }

        if (_HttpServer is not null)
        {
            _HttpServer.StopListening();
            _HttpServer = null;
        }
    }

    private void OnUIProcessDataReceived(object sender, DataReceivedEventArgs args)
    {
        _ModConsole.WriteLine($"{nameof(SceneRecorder)}.{nameof(WebUI)}: {args.Data}", MessageType.Info);
    }

    private void OnUIProcessErrorReceived(object sender, DataReceivedEventArgs args)
    {
        _ModConsole.WriteLine($"{nameof(SceneRecorder)}.{nameof(WebUI)}: {args.Data}", MessageType.Error);
    }
}
