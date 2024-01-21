using System.Runtime.InteropServices;
using UnityEngine;

namespace SceneRecorder.Recording.Domain;

[StructLayout(LayoutKind.Auto)]
public record struct TransformRecord
{
    public Vector3 Position { get; init; }

    public Quaternion Rotation { get; init; }

    public Vector3 Scale { get; init; }

    public Transform? Parent { get; init; }

    public TransformRecord()
    {
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
        Scale = Vector3.one;
        Parent = null;
    }

    public void Apply(Transform transform)
    {
        if (Parent is null)
        {
            transform.position = Position;
            transform.rotation = Rotation;
            transform.localScale = Scale;
            return;
        }

        transform.position = Parent.TransformPoint(Position);
        transform.rotation = Parent.rotation * Rotation;
        transform.localScale = Scale;
    }
}
