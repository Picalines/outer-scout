using OWML.Common;
using OWML.ModHelper;
using SceneRecorder.Application.FFmpeg;
using SceneRecorder.Infrastructure.DependencyInjection;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi;
using UnityEngine;

namespace SceneRecorder;

internal sealed class SceneRecorderMod : ModBehaviour
{
    private WebApiServer? _webApiServer = null;

    private SceneRecorderMod()
    {
        Singleton<IModConfig>.ProvideInstance(() => ModHelper.Config);
        Singleton<IModConsole>.ProvideInstance(() => ModHelper.Console);
    }

    public override void Configure(IModConfig config)
    {
        _webApiServer?.Dispose();

        _webApiServer = new WebApiServer();
    }

    private void Start()
    {
        ModHelper.Console.WriteLine($"{nameof(SceneRecorder)} is loaded!", MessageType.Success);

        if (CheckCompatibility() is false)
        {
            return;
        }

        UnityEngine.Application.runInBackground = true;
        ModHelper.Console.WriteLine("Outer Wilds will run in background", MessageType.Warning);

        Configure(ModHelper.Config);
    }

    private bool CheckCompatibility()
    {
        if (SystemInfo.supportsAsyncGPUReadback is false)
        {
            ModHelper.Console.WriteLine(
                $"async gpu readback is not supported, {nameof(SceneRecorder)} is not available",
                MessageType.Error
            );
            return false;
        }

        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) is false)
        {
            ModHelper.Console.WriteLine(
                $"{RenderTextureFormat.Depth.ToStringWithType()} is not supported, {nameof(SceneRecorder)} is not available",
                MessageType.Error
            );
            return false;
        }

        // TODO: should only block RenderTextureRecorders
        if (FFmpeg.CheckInstallation(ModHelper.Config) is { } checkException)
        {
            ModHelper.Console.WriteLine(
                $"ffmpeg executable not found, {nameof(SceneRecorder)} is not available. See exception below:",
                MessageType.Error
            );

            ModHelper.Console.WriteLine(checkException.ToString(), MessageType.Warning);
            return false;
        }

        return true;
    }
}
