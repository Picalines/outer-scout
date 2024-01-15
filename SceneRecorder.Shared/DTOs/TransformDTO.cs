using Newtonsoft.Json;
using SceneRecorder.Shared.DTOs.JsonConverters;
using UnityEngine;

namespace SceneRecorder.Shared.DTOs;

[JsonConverter(typeof(TransformDTOConverter))]
public readonly record struct TransformDTO
{
    public required Vector3 Position { get; init; }

    public required Quaternion Rotation { get; init; }

    public required Vector3 Scale { get; init; }

    public static TransformDTO FromGlobal(Transform transform) =>
        new()
        {
            Position = transform.position,
            Rotation = transform.rotation,
            Scale = transform.lossyScale,
        };

    public static TransformDTO FromLocal(Transform transform) =>
        new()
        {
            Position = transform.localPosition,
            Rotation = transform.localRotation,
            Scale = transform.localScale,
        };

    public static TransformDTO FromInverse(Transform parentTransform, Transform childTransform) =>
        new()
        {
            Position = parentTransform.InverseTransformPoint(childTransform.position),
            Rotation = parentTransform.InverseTransformRotation(childTransform.rotation),
            Scale = childTransform.lossyScale,
        };

    public void ApplyLocal(Transform transform)
    {
        transform.localPosition = Position;
        transform.localRotation = Rotation;
        transform.localScale = Scale;
    }

    public void ApplyGlobal(Transform transform)
    {
        transform.position = Position;
        transform.rotation = Rotation;
    }
}
