using System.Diagnostics;

namespace SceneRecorder.Recording.FFmpeg;

public static class FFmpeg
{
    public static bool IsInstalled => _IsInstalled.Value;

    public static Exception? InstallationCheckException { get; private set; } = null;

    private static Lazy<bool> _IsInstalled =
        new(() =>
        {
            try
            {
                var process = new Process()
                {
                    StartInfo = new() { FileName = "ffmpeg", Arguments = "-version" }
                };

                process.Start();
                process.WaitForExit();
                return true;
            }
            catch (Exception exception)
            {
                InstallationCheckException = exception;
            }

            return false;
        });
}
