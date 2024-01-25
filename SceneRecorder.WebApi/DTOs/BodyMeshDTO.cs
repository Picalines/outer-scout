namespace SceneRecorder.WebApi.DTOs;

internal sealed class BodyMeshDTO
{
    public required GameObjectDTO Body { get; init; }

    public required IReadOnlyList<SectorMeshDTO> Sectors { get; init; }
}
