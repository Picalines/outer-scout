namespace SceneRecorder.Shared.Models;

public readonly record struct GameObjectDTO
{
    public required string Name { get; init; }

    public required string Path { get; init; }

    public required TransformDTO Transform { get; init; }
}
