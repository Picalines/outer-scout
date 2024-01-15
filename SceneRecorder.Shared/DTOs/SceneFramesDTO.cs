namespace SceneRecorder.Shared.DTOs;

public sealed class SceneFramesDTO
{
    public required int Start { get; init; }

    public required int End { get; init; }

    public required int Rate { get; init; }
}
