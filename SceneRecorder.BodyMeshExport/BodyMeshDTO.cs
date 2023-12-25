using SceneRecorder.Shared.Models;

namespace SceneRecorder.BodyMeshExport;

public sealed class BodyMeshDTO
{
    public required GameObjectDTO Body { get; init; }

    public required IReadOnlyList<SectorMeshDTO> Sectors { get; init; }
}
