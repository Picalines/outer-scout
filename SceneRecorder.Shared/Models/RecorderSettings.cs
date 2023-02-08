using Newtonsoft.Json;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Models;

public sealed class RecorderSettings
{
    [JsonProperty("frame_count")]
    public required int FrameCount { get; init; }

    [JsonProperty("frame_rate")]
    public required int FrameRate { get; init; }

    [JsonProperty("resolution_x")]
    public required int ResolutionX { get; init; }

    [JsonProperty("resolution_y")]
    public required int ResolutionY { get; init; }

    [JsonProperty("hdri_face_size")]
    public required int HdriFaceSize { get; init; }

    [JsonProperty("hide_player_model")]
    public required bool HidePlayerModel { get; init; }

    public (int, int) Resolution
    {
        get => (ResolutionX, ResolutionY);
    }
}
