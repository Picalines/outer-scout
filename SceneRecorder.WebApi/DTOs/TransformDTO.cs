using UnityEngine;

namespace SceneRecorder.WebApi.DTOs;

internal sealed class TransformDTO
{
    public string? Parent { get; init; } = null;

    public Vector3 Position { get; init; } = Vector3.zero;

    public Quaternion Rotation { get; init; } = Quaternion.identity;

    public Vector3 Scale { get; init; } = Vector3.one;
}
