using OuterScout.Application.FFmpeg;
using OuterScout.Shared.DependencyInjection;
using OuterScout.Shared.Extensions;
using OuterScout.WebApi;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace OuterScout;

internal sealed class OuterScoutMod : ModBehaviour
{
    private WebApiServer? _webApiServer = null;

    private OuterScoutMod()
    {
        Singleton<IModConfig>.ProvideInstance(() => ModHelper.Config);
        Singleton<IModManifest>.ProvideInstance(() => ModHelper.Manifest);
        Singleton<IModConsole>.ProvideInstance(() => ModHelper.Console);
    }

    public override void Configure(IModConfig config)
    {
        _webApiServer?.Dispose();

        _webApiServer = new WebApiServer();
    }

    private void Start()
    {
        ModHelper.Console.WriteLine($"{nameof(OuterScout)} is loaded!", MessageType.Success);

        ShowCompatabilityWarnings();

        UnityEngine.Application.runInBackground = true;
        ModHelper.Console.WriteLine("Outer Wilds will run in background", MessageType.Warning);

        Configure(ModHelper.Config);
    }

    private void ShowCompatabilityWarnings()
    {
        if (SystemInfo.supportsAsyncGPUReadback is false)
        {
            AddWarning("async gpu readback is not supported, texture recording is not available");
        }

        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) is false)
        {
            AddWarning(
                $"{RenderTextureFormat.Depth.ToStringWithType()} is not supported, depth recording is not available"
            );
        }

        if (FFmpeg.CheckInstallation() is { } exception)
        {
            AddWarning(
                "ffmpeg executable not found, texture recording is not available. See console for more details"
            );

            ModHelper.Console.WriteLine(exception.ToString(), MessageType.Warning);
        }

        void AddWarning(string message)
        {
            ModHelper.MenuHelper.PopupMenuManager.RegisterStartupPopup($"Outer Scout: {message}");
            ModHelper.Console.WriteLine(message, MessageType.Warning);
        }
    }
}
