﻿using OWML.Common;
using OWML.ModHelper;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders;
using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.WebApi;
using UnityEngine.InputSystem;

namespace Picalines.OuterWilds.SceneRecorder;

internal sealed class SceneRecorderMod : ModBehaviour
{
    private const Key _OpenWebUIKey = Key.F9;

    private OutputRecorder _OutputRecorder = null!;

    private WebApiServer? _WebApiServer = null;

    public override void Configure(IModConfig config)
    {
        if (_OutputRecorder is not null)
        {
            _OutputRecorder.ModConsole = config.GetSettingsValue<bool>("FFmpeg logs")
                ? ModHelper.Console
                : ModHelper.Console.WithFiltering((line, _) => !line.StartsWith("FFmpeg: "));
        }

        _WebApiServer?.Configure(ModHelper.Config, ModHelper.Console);
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
