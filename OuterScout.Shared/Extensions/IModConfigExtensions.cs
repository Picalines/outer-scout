using OWML.Common;

namespace OuterScout.Shared.Extensions;

public static class IModConfigExtensions
{
    public static int GetApiPortSetting(this IModConfig modConfig) =>
        modConfig.GetSettingsValue<int>("apiPort");

    public static string GetFFmpegExecutablePathSetting(this IModConfig? modConfig) =>
        modConfig?.GetSettingsValue<string>("ffmpegPath") ?? "ffmpeg";

    public static bool GetEnableApiInfoLogsSetting(this IModConfig modConfig) =>
        modConfig.GetSettingsValue<bool>("enableApiInfoLogs");

    public static bool GetEnableFFmpegLogsSetting(this IModConfig modConfig) =>
        modConfig.GetSettingsValue<bool>("enableFFmpegLogs");
}
