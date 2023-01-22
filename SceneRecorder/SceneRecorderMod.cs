using Newtonsoft.Json;
using OWML.Common;
using OWML.ModHelper;
using Picalines.OuterWilds.SceneRecorder.Json;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders;
using Picalines.OuterWilds.SceneRecorder.Utils;
using Picalines.OuterWilds.SceneRecorder.WebInterop;
using UnityEngine.InputSystem;

namespace Picalines.OuterWilds.SceneRecorder;

internal sealed class SceneRecorderMod : ModBehaviour
{
    private const Key _RecordKey = Key.F9;

    private string _SceneSettingsPath = null!;

    private SceneSettings _SceneSettings = null!;

    private OutputRecorder _OutputRecorder = null!;

    private WebApiServer? _WebApiServer = null;

    private WebUIHost? _WebUIHost = null;

    private readonly LazyUnityReference<OWRigidbody> _Player = new(Locator.GetPlayerBody);

    private readonly LazyUnityReference<OWCamera> _PlayerCamera = new(Locator.GetPlayerCamera);

    private readonly LazyUnityReference<PlayerAudioController> _PlayerAudioController = new(Locator.GetPlayerAudioController);

    private readonly LazyUnityReference<OWCamera> _FreeCamera = LazyUnityReference.FromFind<OWCamera>("FREECAM");

    public override void Configure(IModConfig config)
    {
        _WebUIHost?.Dispose();
        _WebApiServer?.Dispose();

        try
        {
            _SceneSettingsPath = config.GetSettingsValue<string>("owscene_path");
            _SceneSettings = JsonConvert.DeserializeObject<SceneSettings>(File.ReadAllText(_SceneSettingsPath))!;
        }
        catch (Exception exception)
        {
            ModHelper.Console.WriteLine($"Invalid {nameof(SceneRecorder)} settings. See exception:", MessageType.Error);
            ModHelper.Console.WriteLine(exception.ToString(), MessageType.Error);
        }

        _WebApiServer = new WebApiServer(config, _OutputRecorder);

        _WebUIHost = new WebUIHost(config, ModHelper.Console);
    }

    private void Start()
    {
        ModHelper.Console.WriteLine($"{nameof(SceneRecorder)} is loaded!", MessageType.Success);

        _OutputRecorder = gameObject.AddComponent<OutputRecorder>();
    }

    private bool ListenToInput
    {
        get => _Player.ObjectExists;
    }

    private void OnDestroy()
    {
        _WebUIHost?.Dispose();
        _WebApiServer?.Dispose();

        Destroy(_OutputRecorder);
    }

    private void Update()
    {
        if (ListenToInput is false)
        {
            return;
        }

        if (Keyboard.current[_RecordKey].isPressed)
        {
            TryStartRecording();
        }
        else
        {
            TryStopRecording();
        }
    }

    private void TryStartRecording()
    {
        if (_OutputRecorder == null || _OutputRecorder is { enabled: true } or { IsAbleToRecord: false })
        {
            return;
        }

        gameObject.SetActive(false);

        _OutputRecorder.Configure(ModHelper.Console, _SceneSettings, Path.GetDirectoryName(_SceneSettingsPath));

        gameObject.SetActive(true);

        _OutputRecorder.enabled = true;
    }

    private void TryStopRecording()
    {
        if (!(_OutputRecorder != null && _OutputRecorder.enabled))
        {
            return;
        }

        _OutputRecorder.enabled = false;
    }
}
