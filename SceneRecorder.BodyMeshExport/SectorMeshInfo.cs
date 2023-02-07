using Newtonsoft.Json;

namespace Picalines.OuterWilds.SceneRecorder.BodyMeshExport;

public sealed class SectorMeshInfo
{
    [JsonProperty("path")]
    public required string Path { get; init; }

    [JsonProperty("plain_meshes")]
    public required IReadOnlyList<MeshInfo> PlainMeshes { get; init; }

    [JsonProperty("streamed_meshes")]
    public required IReadOnlyList<MeshInfo> StreamedMeshes { get; init; }
}
