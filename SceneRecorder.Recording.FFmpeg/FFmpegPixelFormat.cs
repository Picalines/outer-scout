namespace SceneRecorder.Recording.FFmpeg;

public enum FFmpegPixelFormat
{
    RGBA,
    GrayF32LE,
    YUV420P,
}

public static class FFmpegPixelFormatExtensions
{
    public static string ToCLIOption(this FFmpegPixelFormat pixelFormat)
    {
        return pixelFormat switch
        {
            FFmpegPixelFormat.RGBA => "rgba",
            FFmpegPixelFormat.GrayF32LE => "grayf32le",
            FFmpegPixelFormat.YUV420P => "yuv420p",
            _ => throw new NotImplementedException(),
        };
    }
}
