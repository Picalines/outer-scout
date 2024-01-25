using SceneRecorder.Infrastructure;
using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.Recording.Domain;

public sealed class SceneSettings
{
    public required string OutputDirectory
    {
        get => _outputDirectory;
        init
        {
            value.Throw().IfNullOrWhiteSpace();
            _outputDirectory = value;
        }
    }

    public required IntRange FrameRange { get; init; }

    public required int FrameRate
    {
        get => _frameRate;
        init
        {
            value.Throw().IfLessThan(1);
            _frameRate = value;
        }
    }

    public required bool HidePlayerModel { get; init; }

    public int NumberOfFrames => FrameRange.Length + 1;

    private string _outputDirectory = null!;

    private int _frameRate;
}
