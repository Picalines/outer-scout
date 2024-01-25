namespace SceneRecorder.WebApi.DTOs;

internal sealed class RecorderStatusResponse
{
    public required bool Enabled { get; init; }

    public required bool IsAbleToRecord { get; init; }

    public required int FramesRecorded { get; init; }
}
