using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.Shared.Models;

namespace Picalines.OuterWilds.SceneRecorder.BodyMeshExport;

public sealed class GroundBodyMeshInfo
{
    [JsonProperty("body_name")]
    public required string BodyName { get; init; }

    [JsonProperty("body_transform")]
    public required TransformModel BodyTransform { get; init; }

    [JsonProperty("sectors")]
    public required IReadOnlyList<SectorMeshInfo> Sectors { get; init; }
}
