using OWML.Common;
using OWML.ModHelper;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders;
using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.Shared.Interfaces;
using Picalines.OuterWilds.SceneRecorder.WebApi;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder;

internal sealed class SceneRecorderMod : ModBehaviour
{
    private const string CommonCameraAPIModId = "xen.CommonCameraUtility";

    private OutputRecorder _OutputRecorder = null!;

    private WebApiServer? _WebApiServer = null;

    private PlayerCameraEnabler? _PlayerCameraEnabler = null;

    public override void Configure(IModConfig config)
    {
        if (_OutputRecorder is not null)
        {
            _OutputRecorder.ModConsole = config.GetSettingsValue<bool>("FFmpeg logs")
                ? ModHelper.Console
                : ModHelper.Console.WithFiltering((line, _) => !line.StartsWith("FFmpeg: "));

            var commonCameraAPI = ModHelper.Interaction.TryGetModApi<ICommonCameraAPI>(
                CommonCameraAPIModId
            );

            if (commonCameraAPI is null)
            {
                ModHelper.Console.WriteLine(
                    $"{CommonCameraAPIModId} is required for {nameof(SceneRecorder)}",
                    MessageType.Error
                );
                return;
            }

            _OutputRecorder.CommonCameraAPI = commonCameraAPI;
        }

        _WebApiServer?.Configure(config, ModHelper.Console);

        _PlayerCameraEnabler?.Configure(config);
    }

    private void Start()
    {
        ModHelper.Console.WriteLine($"{nameof(SceneRecorder)} is loaded!", MessageType.Success);

        Application.runInBackground = true;
        ModHelper.Console.WriteLine("Outer Wilds will run in background", MessageType.Warning);

        _OutputRecorder = gameObject.AddComponent<OutputRecorder>();

        _WebApiServer = gameObject.AddComponent<WebApiServer>();

        _PlayerCameraEnabler = gameObject.AddComponent<PlayerCameraEnabler>();

        Configure(ModHelper.Config);
    }
}
