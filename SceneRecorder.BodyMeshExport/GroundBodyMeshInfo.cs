using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.Shared.Models;

namespace Picalines.OuterWilds.SceneRecorder.BodyMeshExport;

public sealed class GroundBodyMeshInfo
{
    [JsonProperty("body_name")]
    public string BodyName { get; private set; }

    [JsonProperty("body_transform")]
    public TransformModel BodyTransform { get; private set; }

    [JsonProperty("plain_meshes")]
    public IReadOnlyList<MeshInfo> PlainMeshes { get; private set; }

    [JsonProperty("streamed_meshes")]
    public IReadOnlyList<MeshInfo> StreamedMeshes { get; private set; }

    public GroundBodyMeshInfo(
        string bodyName,
        TransformModel bodyTransform,
        IReadOnlyList<MeshInfo> plainMeshes,
        IReadOnlyList<MeshInfo> streamedMeshes)
    {
        BodyName = bodyName;
        BodyTransform = bodyTransform;
        PlainMeshes = plainMeshes;
        StreamedMeshes = streamedMeshes;
    }
}
