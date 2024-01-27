using OWML.Common;
using OWML.ModHelper;
using SceneRecorder.Infrastructure.API;
using SceneRecorder.Infrastructure.DependencyInjection;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Recording.FFmpeg;
using SceneRecorder.Recording.Recorders;
using SceneRecorder.WebApi;
using UnityEngine;

namespace SceneRecorder;

internal sealed class SceneRecorderMod : ModBehaviour
{
    private const string CommonCameraAPIModId = "xen.CommonCameraUtility";

    private OutputRecorder _OutputRecorder = null!;

    private WebApiServer? _WebApiServer = null;

    private SceneRecorderMod()
    {
        Singleton<IModConfig>.ProvideInstance(() => ModHelper.Config);
        Singleton<IModConsole>.ProvideInstance(() => ModHelper.Console);
    }

    public override void Configure(IModConfig config)
    {
        if (_OutputRecorder is not null)
        {
            _OutputRecorder.ModConfig = config;
            _OutputRecorder.ModConsole = ModHelper.Console;

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
    }

    private void Start()
    {
        ModHelper.Console.WriteLine($"{nameof(SceneRecorder)} is loaded!", MessageType.Success);

        if (SystemInfo.supportsAsyncGPUReadback is false)
        {
            ModHelper.Console.WriteLine(
                $"async gpu readback is not supported, {nameof(SceneRecorder)} is not available",
                MessageType.Error
            );

            return;
        }

        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) is false)
        {
            ModHelper.Console.WriteLine(
                $"{RenderTextureFormat.Depth.ToStringWithType()} is not supported, {nameof(SceneRecorder)} is not available",
                MessageType.Error
            );

            return;
        }

        if (FFmpeg.CheckInstallation(ModHelper.Config) is { } checkException)
        {
            ModHelper.Console.WriteLine(
                $"ffmpeg executable not found, {nameof(SceneRecorder)} is not available. See exception below:",
                MessageType.Error
            );

            ModHelper.Console.WriteLine(checkException.ToString(), MessageType.Warning);

            return;
        }

        Application.runInBackground = true;
        ModHelper.Console.WriteLine("Outer Wilds will run in background", MessageType.Warning);

        _OutputRecorder = gameObject.AddComponent<OutputRecorder>();

        _WebApiServer = gameObject.AddComponent<WebApiServer>();

        Configure(ModHelper.Config);
    }
}
