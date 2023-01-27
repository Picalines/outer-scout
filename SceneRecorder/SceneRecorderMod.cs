﻿using Newtonsoft.Json;
using OWML.Common;
using OWML.ModHelper;
using Picalines.OuterWilds.SceneRecorder.Json;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders;
using Picalines.OuterWilds.SceneRecorder.WebApi;
using UnityEngine.InputSystem;

namespace Picalines.OuterWilds.SceneRecorder;

internal sealed class SceneRecorderMod : ModBehaviour
{
    private const Key _OpenWebUIKey = Key.F9;

    private string _SceneSettingsPath = null!;

    private SceneSettings _SceneSettings = null!;

    private OutputRecorder _OutputRecorder = null!;

    private WebApiServer? _WebApiServer = null;

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

        if (_OutputRecorder is not null)
        {
            _OutputRecorder.ModConsole = ModHelper.Console;
            _OutputRecorder.SceneSettings = _SceneSettings;
            _OutputRecorder.OutputDirectory = Path.GetDirectoryName(_SceneSettingsPath);
        }

        _WebApiServer?.Configure(ModHelper.Config);
    }

    private void Start()
    {
        ModHelper.Console.WriteLine($"{nameof(SceneRecorder)} is loaded!", MessageType.Success);

        _OutputRecorder = gameObject.AddComponent<OutputRecorder>();

        _WebApiServer = gameObject.AddComponent<WebApiServer>();

        Configure(ModHelper.Config);
    }

    private void OnDestroy()
    {
        Destroy(_WebApiServer);
        Destroy(_OutputRecorder);
    }
}