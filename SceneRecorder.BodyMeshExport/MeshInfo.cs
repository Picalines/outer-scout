using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.Shared.Models;

namespace Picalines.OuterWilds.SceneRecorder.BodyMeshExport;

public sealed class MeshInfo
{
    [JsonProperty("path")]
    public required string Path { get; init; } // GameObject path for "static" meshes, Asset path for streamed ones

    [JsonProperty("transform")]
    public required TransformModel Transform { get; init; }
}
