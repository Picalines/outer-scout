using Newtonsoft.Json;

namespace Picalines.OuterWilds.SceneRecorder.Models;

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
