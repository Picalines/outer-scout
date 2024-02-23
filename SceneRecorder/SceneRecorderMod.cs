﻿using OWML.Common;
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

        ShowCompatabilityWarnings();

        UnityEngine.Application.runInBackground = true;
        ModHelper.Console.WriteLine("Outer Wilds will run in background", MessageType.Warning);

        Configure(ModHelper.Config);
    }

    private void ShowCompatabilityWarnings()
    {
        if (SystemInfo.supportsAsyncGPUReadback is false)
        {
            ModHelper.Console.WriteLine(
                $"async gpu readback is not supported, texture recording is not available",
                MessageType.Warning
            );
        }

        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) is false)
        {
            ModHelper.Console.WriteLine(
                $"{RenderTextureFormat.Depth.ToStringWithType()} is not supported, depth recording is not available",
                MessageType.Warning
            );
        }

        if (FFmpeg.CheckInstallation() is { } exception)
        {
            ModHelper.Console.WriteLine(
                $"ffmpeg executable not found, texture recording is not available. See exception below:",
                MessageType.Warning
            );

            ModHelper.Console.WriteLine(exception.ToString(), MessageType.Warning);
        }
    }
}
