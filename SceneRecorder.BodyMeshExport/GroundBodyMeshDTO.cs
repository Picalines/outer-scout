using SceneRecorder.Shared.Models;

namespace SceneRecorder.BodyMeshExport;

public sealed class GroundBodyMeshDTO
{
    public required string BodyName { get; init; }

    public required TransformDTO BodyTransform { get; init; }

    public required IReadOnlyList<SectorMeshInfo> Sectors { get; init; }
}
