using Newtonsoft.Json;
using OWML.Common;
using OWML.ModHelper;
using Picalines.OuterWilds.SceneRecorder.Json;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders;
using Picalines.OuterWilds.SceneRecorder.WebApi;
using Picalines.OuterWilds.SceneRecorder.WebUI;
using UnityEngine.InputSystem;

namespace Picalines.OuterWilds.SceneRecorder;

internal sealed class SceneRecorderMod : ModBehaviour
{
    private const Key _OpenWebUIKey = Key.F9;

    private string _SceneSettingsPath = null!;

    private SceneSettings _SceneSettings = null!;

    private OutputRecorder _OutputRecorder = null!;

    private WebApiServer? _WebApiServer = null;

    private WebUIHost? _WebUIHost = null;

    public override void Configure(IModConfig config)
    {
        try
        {
            _SceneSettingsPath = config.GetSettingsValue<string>("owscene_path");
            _SceneSettings = JsonConvert.DeserializeObject<SceneSettings>(File.ReadAllText(_SceneSettingsPath))!;
        }
        catch (Exception exception)
        {
            ModHelper.Console.WriteLine($"Invalid .owscene file. See exception:", MessageType.Error);
            ModHelper.Console.WriteLine(exception.ToString(), MessageType.Error);
        }

        ConfigureComponents();
    }

    private void Start()
    {
        ModHelper.Console.WriteLine($"{nameof(SceneRecorder)} is loaded!", MessageType.Success);

        _OutputRecorder = gameObject.AddComponent<OutputRecorder>();
        _OutputRecorder.BeforeRecordingStarted += () =>
        {
            _OutputRecorder.ModConsole = ModHelper.Console;
            _OutputRecorder.SceneSettings = _SceneSettings;
            _OutputRecorder.OutputDirectory = Path.GetDirectoryName(_SceneSettingsPath);
        };

        _WebApiServer = gameObject.AddComponent<WebApiServer>();

        _WebUIHost = gameObject.AddComponent<WebUIHost>();

        ConfigureComponents();
    }

    private void ConfigureComponents()
    {
        _WebApiServer?.Configure(ModHelper.Config);
        _WebUIHost?.Configure(ModHelper.Config, ModHelper.Console);
    }

    private void OnDestroy()
    {
        Destroy(_WebUIHost);
        Destroy(_WebApiServer);
        Destroy(_OutputRecorder);
    }

    private void Update()
    {
        if (Keyboard.current[_OpenWebUIKey].wasPressedThisFrame)
        {
            if (_OutputRecorder.IsAbleToRecord is false)
            {
                Locator.GetPlayerAudioController().PlayNegativeUISound();
                return;
            }

            _WebUIHost?.ToggleBrowser();
        }
    }
}
