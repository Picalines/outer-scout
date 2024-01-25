namespace SceneRecorder.WebApi.DTOs;

internal sealed class SceneSettingsDTO
{
    public required string OutputDirectory { get; init; }

    public required SceneFramesDTO Frames { get; init; }

    public required bool HidePlayerModel { get; init; }
}
