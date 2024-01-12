namespace SceneRecorder.Shared.Models;

public sealed class RecorderSettingsDTO
{
    public required string OutputDirectory { get; init; }

    public required int StartFrame { get; init; }

    public required int EndFrame { get; init; }

    public required int FrameRate { get; init; }

    public required int ResolutionX { get; init; }

    public required int ResolutionY { get; init; }

    public required bool RecordHdri { get; init; }

    public required bool RecordDepth { get; init; }

    public required int HdriFaceSize { get; init; }

    public required bool HidePlayerModel { get; init; }

    public (int, int) Resolution
    {
        get => (ResolutionX, ResolutionY);
    }
}
