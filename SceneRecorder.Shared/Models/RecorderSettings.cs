using Newtonsoft.Json;

namespace SceneRecorder.Shared.Models;

public sealed class RecorderSettings
{
    [JsonProperty("output_directory")]
    public required string OutputDirectory { get; init; }

    [JsonProperty("start_frame")]
    public required int StartFrame { get; init; }

    [JsonProperty("end_frame")]
    public required int EndFrame { get; init; }

    [JsonProperty("frame_rate")]
    public required int FrameRate { get; init; }

    [JsonProperty("resolution_x")]
    public required int ResolutionX { get; init; }

    [JsonProperty("resolution_y")]
    public required int ResolutionY { get; init; }

    [JsonProperty("record_hdri")]
    public required bool RecordHdri { get; init; }

    [JsonProperty("record_depth")]
    public required bool RecordDepth { get; init; }

    [JsonProperty("hdri_face_size")]
    public required int HdriFaceSize { get; init; }

    [JsonProperty("hide_player_model")]
    public required bool HidePlayerModel { get; init; }

    [JsonProperty("show_progress_gui")]
    public required bool ShowProgressGUI { get; init; }

    public (int, int) Resolution
    {
        get => (ResolutionX, ResolutionY);
    }
}
