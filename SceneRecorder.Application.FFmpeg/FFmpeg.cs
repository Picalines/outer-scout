using System.Diagnostics;
using OWML.Common;
using SceneRecorder.Infrastructure.DependencyInjection;
using SceneRecorder.Infrastructure.Extensions;

namespace SceneRecorder.Application.FFmpeg;

public static class FFmpeg
{
    private static Exception? _cachedCheckException = null;

    private static bool _isInstallationChecked = false;

    public static Exception? CheckInstallation()
    {
        if (_isInstallationChecked)
        {
            return _cachedCheckException;
        }

        try
        {
            var process = new Process()
            {
                StartInfo = new()
                {
                    FileName = Singleton<IModConfig>.Instance.GetFFmpegExecutablePathSetting(),
                    Arguments = "-version"
                }
            };

            process.Start();
            process.WaitForExit();
            return null;
        }
        catch (Exception exception)
        {
            return _cachedCheckException = exception;
        }
        finally
        {
            _isInstallationChecked = true;
        }
    }

    public static void ThrowIfNotAvailable()
    {
        if (CheckInstallation() is { } exception)
        {
            throw new InvalidOperationException("ffmpeg is not available", exception);
        }
    }
}
