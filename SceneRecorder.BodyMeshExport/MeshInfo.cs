using Newtonsoft.Json;
using SceneRecorder.Shared.Models;

namespace SceneRecorder.BodyMeshExport;

public sealed class MeshInfo
{
    [JsonProperty("path")]
    public required string Path { get; init; } // GameObject path for "static" meshes, Asset path for streamed ones

    [JsonProperty("transform")]
    public required TransformModel Transform { get; init; }
}
