using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Json;
using System.Diagnostics;
using System.Reflection;

namespace Picalines.OuterWilds.SceneRecorder.WebInterop;

internal sealed class WebUIHost : IDisposable
{
    private readonly IModConsole _ModConsole;

    private Process? _UIProcess;

    public WebUIHost(IModConsole modConsole, SceneRecorderSettings settings)
    {
        _ModConsole = modConsole;

        _UIProcess = Process.Start(new ProcessStartInfo()
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = "dotnet",
            WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "WebUI"),
            Arguments = $"OuterWilds.SceneRecorder.WebUI.dll --urls \"{settings.WebUIUrl}\" --api-url \"{settings.WebApiUrl}\"",
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });

        _UIProcess.OutputDataReceived += OnUIProcessDataReceived;
        _UIProcess.ErrorDataReceived += OnUIProcessErrorReceived;
        _UIProcess.EnableRaisingEvents = true;
        _UIProcess.BeginOutputReadLine();
    }

    public void Dispose()
    {
        if (_UIProcess is null)
        {
            return;
        }

        if (_UIProcess.HasExited is false)
        {
            _UIProcess.Kill();
        }

        _UIProcess.Dispose();
        _UIProcess = null;
    }

    private void OnUIProcessDataReceived(object sender, DataReceivedEventArgs args)
    {
        _ModConsole.WriteLine($"{nameof(SceneRecorder)}.{nameof(WebUIHost)}: {args.Data}", MessageType.Info);
    }

    private void OnUIProcessErrorReceived(object sender, DataReceivedEventArgs args)
    {
        _ModConsole.WriteLine($"{nameof(SceneRecorder)}.{nameof(WebUIHost)}: {args.Data}", MessageType.Error);
    }
}
