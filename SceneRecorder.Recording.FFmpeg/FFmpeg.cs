using System.Diagnostics;
using OWML.Common;
using SceneRecorder.Shared.Extensions;

namespace SceneRecorder.Recording.FFmpeg;

public static class FFmpeg
{
    public static Exception? CheckInstallation(IModConfig modConfig)
    {
        try
        {
            var process = new Process()
            {
                StartInfo = new()
                {
                    FileName = modConfig.GetFFmpegExecutablePathSetting(),
                    Arguments = "-version"
                }
            };

            process.Start();
            process.WaitForExit();
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }
}
