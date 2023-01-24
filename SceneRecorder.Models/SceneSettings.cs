#if IS_TARGET_MOD
using Newtonsoft.Json;
#else
using JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;
#endif

namespace Picalines.OuterWilds.SceneRecorder.Json;

#pragma warning disable CS8618

public sealed class SceneSettings
{
    [JsonProperty("ground_body_name")]
    public string GroundBodyName { get; init; }

    [JsonProperty("frame_count")]
    public int FrameCount { get; init; }

    [JsonProperty("frame_rate")]
    public int FrameRate { get; init; }

    [JsonProperty("resolution_x")]
    public int ResolutionX { get; init; }

    [JsonProperty("resolution_y")]
    public int ResolutionY { get; init; }

    [JsonProperty("hdri_face_size")]
    public int HDRIFaceSize { get; init; }

    [JsonProperty("hide_player_model")]
    public bool HidePlayerModel { get; init; }

    public (int, int) Resolution
    {
        get => (ResolutionX, ResolutionY);
    }
}
