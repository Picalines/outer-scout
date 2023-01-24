using OWML.Common;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.WebUI;

public sealed class WebUIHost : MonoBehaviour
{
    private IModConsole _ModConsole = null!;

    private string _Url = null!;

    private Process? _UIProcess;

    private bool _IsBrowserOpened = false;

    public void Configure(IModConfig modConfig, IModConsole modConsole)
    {
        OnDestroy();

        _ModConsole = modConsole;

        _Url = $"http://localhost:{modConfig.GetSettingsValue<int>("web_ui_port")}/";
        var apiUrl = $"http://localhost:{modConfig.GetSettingsValue<int>("web_api_port")}/";

        _UIProcess = Process.Start(new ProcessStartInfo()
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            FileName = "dotnet",
            WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "WebUI.BlazorApp"),
            Arguments = $"SceneRecorder.WebUI.BlazorApp.dll --urls \"{_Url}\" --api-url \"{apiUrl}\"",
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });

        _UIProcess.OutputDataReceived += OnUIProcessDataReceived;
        _UIProcess.ErrorDataReceived += OnUIProcessErrorReceived;
        _UIProcess.EnableRaisingEvents = true;
        _UIProcess.BeginOutputReadLine();
    }

    private void OnDestroy()
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

    public void ToggleBrowser()
    {
        _IsBrowserOpened = !_IsBrowserOpened;

        Screen.fullScreen = !_IsBrowserOpened;

        if (_IsBrowserOpened)
        {
            Application.OpenURL(_Url);
        }
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
