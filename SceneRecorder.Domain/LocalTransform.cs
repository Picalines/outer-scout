using System.Runtime.InteropServices;
using UnityEngine;

namespace SceneRecorder.Domain;

[StructLayout(LayoutKind.Auto)]
public record struct LocalTransform
{
    public Vector3 Position { get; init; }

    public Quaternion Rotation { get; init; }

    public Vector3 Scale { get; init; }

    public Transform? Parent { get; init; }

    public LocalTransform()
    {
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
        Scale = Vector3.one;
        Parent = null;
    }

    public void Deconstruct(
        out Vector3 position,
        out Quaternion rotation,
        out Vector3 scale,
        out Transform? parent
    )
    {
        position = Position;
        rotation = Rotation;
        scale = Scale;
        parent = Parent;
    }
}
