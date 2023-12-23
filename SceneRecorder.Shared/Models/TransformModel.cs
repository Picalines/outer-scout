using Newtonsoft.Json;
using SceneRecorder.Shared.Models.JsonConverters;
using UnityEngine;

namespace SceneRecorder.Shared.Models;

[JsonConverter(typeof(TransformModelConverter))]
public readonly record struct TransformModel
{
    public required Vector3 Position { get; init; }

    public required Quaternion Rotation { get; init; }

    public required Vector3 Scale { get; init; }

    public static TransformModel FromGlobalTransform(Transform transform)
    {
        return new TransformModel
        {
            Position = transform.position,
            Rotation = transform.rotation,
            Scale = transform.lossyScale,
        };
    }

    public static TransformModel FromLocalTransform(Transform transform)
    {
        return new TransformModel
        {
            Position = transform.localPosition,
            Rotation = transform.localRotation,
            Scale = transform.localScale,
        };
    }

    public static TransformModel FromInverse(Transform parentTransform, Transform childTransform)
    {
        return new()
        {
            Position = parentTransform.InverseTransformPoint(childTransform.position),
            Rotation = parentTransform.InverseTransformRotation(childTransform.rotation),
            Scale = childTransform.lossyScale,
        };
    }

    public void ApplyToLocalTransform(Transform transform)
    {
        transform.localPosition = Position;
        transform.localRotation = Rotation;
        transform.localScale = Scale;
    }

    public void ApplyToGlobalPositionAndRotation(Transform transform)
    {
        transform.position = Position;
        transform.rotation = Rotation;
    }
}
