namespace SceneRecorder.WebApi.RouteMappers.DTOs;

public sealed class SectorMeshDTO
{
    public required string Path { get; init; }

    public required IReadOnlyList<MeshDTO> PlainMeshes { get; init; }

    public required IReadOnlyList<MeshDTO> StreamedMeshes { get; init; }
}
