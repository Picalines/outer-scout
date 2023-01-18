using Microsoft.AspNetCore.SignalR.Client;
using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.WebInterop;

internal sealed class WebUI : IDisposable
{
    private readonly IModConsole _ModConsole;

    private Process? _UIProcess;

    private HubConnection? _HubConnection;

    public WebUI(IModConsole modConsole, SceneRecorderSettings settings)
    {
        _ModConsole = modConsole;

        var uiUrl = $"http://localhost:{settings.WebUIPort}";

        _UIProcess = Process.Start(new ProcessStartInfo()
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = "dotnet",
            WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "WebUI"),
            Arguments = $"OuterWilds.SceneRecorder.WebUI.dll --urls \"{uiUrl}\"",
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });

        _UIProcess.OutputDataReceived += OnUIProcessDataReceived;
        _UIProcess.ErrorDataReceived += OnUIProcessErrorReceived;
        _UIProcess.EnableRaisingEvents = true;
        _UIProcess.BeginOutputReadLine();

        _HubConnection = new HubConnectionBuilder()
            .WithUrl($"{uiUrl}/signalr/")
            .Build()
            .LogOnClosed(_ModConsole);

        ConfigureHubConnection();

        Task.Run(async () =>
        {
            await _HubConnection.StartAsyncWithLogs(_ModConsole);
        });
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

        if (_HubConnection is not null)
        {
            _HubConnection.StopAsync().Wait();
            _HubConnection = null;
        }
    }

    private void ConfigureHubConnection()
    {
        _HubConnection!.On("GetTimeSinceStartup", async (string message) =>
        {
            await _HubConnection!.SendAsync("ReceiveTimeSinceStartup", Time.realtimeSinceStartup);
        });
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
