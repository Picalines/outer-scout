using UnityEngine;

namespace OuterScout.WebApi.DTOs;

internal sealed class TransformDTO
{
    public string? Parent { get; init; }

    public Vector3? Position { get; init; }

    public Quaternion? Rotation { get; init; }

    public Vector3? Scale { get; init; }
}
