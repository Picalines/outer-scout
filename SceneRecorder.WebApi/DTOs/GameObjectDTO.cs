namespace SceneRecorder.WebApi.DTOs;

internal readonly record struct GameObjectDTO
{
    public required string Name { get; init; }

    public required string Path { get; init; }

    public required TransformDTO Transform { get; init; }
}
