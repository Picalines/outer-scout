using SceneRecorder.Shared.DTOs;

namespace SceneRecorder.WebApi.RouteMappers.DTOs;

public sealed class BodyMeshDTO
{
    public required GameObjectDTO Body { get; init; }

    public required IReadOnlyList<SectorMeshDTO> Sectors { get; init; }
}
