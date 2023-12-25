using SceneRecorder.Shared.Models;

namespace SceneRecorder.BodyMeshExport;

public sealed class MeshDTO
{
    // GameObject path for "static" meshes, Asset path for streamed ones
    public required string Path { get; init; }

    public required TransformDTO GlobalTransform { get; init; }

    public required TransformDTO LocalTransform { get; init; }
}
