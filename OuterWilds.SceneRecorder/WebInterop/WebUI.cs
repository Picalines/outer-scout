using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Picalines.OuterWilds.SceneRecorder.WebInterop;

internal sealed class WebUI : IDisposable
{
    private readonly IModConsole _ModConsole;

    private Process? _UIProcess;

    public WebUI(IModConsole modConsole, SceneRecorderSettings settings)
    {
        _ModConsole = modConsole;

        _UIProcess = Process.Start(new ProcessStartInfo()
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = "dotnet",
            WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            Arguments = $"OuterWilds.SceneRecorder.WebUI.dll --urls \"http://localhost:{settings.WebUIPort}\"",
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
        if (_UIProcess is not null)
        {
            if (_UIProcess.HasExited is false)
            {
                _UIProcess.Kill();
            }

            _UIProcess.Dispose();
            _UIProcess = null;
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
