using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.Models;

namespace Picalines.OuterWilds.SceneRecorder.BodyMeshExport;

public sealed class MeshInfo
{
    [JsonProperty("path")]
    public string Path { get; private set; } // GameObject path for "static" meshes, Asset path for streamed ones

    [JsonProperty("transform")]
    public TransformModel Transform { get; private set; }

    public MeshInfo(string path, TransformModel transform)
    {
        Path = path;
        Transform = transform;
    }
}
