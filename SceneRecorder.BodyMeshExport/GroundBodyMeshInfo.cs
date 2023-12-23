using Newtonsoft.Json;
using SceneRecorder.Shared.Models;

namespace SceneRecorder.BodyMeshExport;

public sealed class GroundBodyMeshInfo
{
    [JsonProperty("body_name")]
    public required string BodyName { get; init; }

    [JsonProperty("body_transform")]
    public required TransformModel BodyTransform { get; init; }

    [JsonProperty("sectors")]
    public required IReadOnlyList<SectorMeshInfo> Sectors { get; init; }
}
