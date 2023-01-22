using Newtonsoft.Json;

namespace Picalines.OuterWilds.SceneRecorder.Json;

#pragma warning disable CS8618

public sealed class SceneSettings
{
    [JsonProperty("ground_body_name")]
    public string GroundBodyName { get; private set; }

    [JsonProperty("frame_count")]
    public int FrameCount { get; private set; }

    [JsonProperty("frame_rate")]
    public int FrameRate { get; private set; }

    [JsonProperty("resolution_x")]
    public int ResolutionX { get; private set; }

    [JsonProperty("resolution_y")]
    public int ResolutionY { get; private set; }

    [JsonProperty("hdri_face_size")]
    public int HDRIFaceSize { get; private set; }

    [JsonProperty("hide_player_model")]
    public bool HidePlayerModel { get; private set; }

    private SceneSettings()
    {
    }
}
