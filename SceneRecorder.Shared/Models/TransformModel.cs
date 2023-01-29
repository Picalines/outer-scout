using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.Shared.Models.JsonConverters;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Models;

[JsonConverter(typeof(TransformModelConverter))]
public record struct TransformModel(
    Vector3 Position,
    Quaternion Rotation,
    Vector3 Scale)
{
    public static TransformModel FromGlobalTransform(Transform transform)
    {
        return new TransformModel(transform.position, transform.rotation, transform.lossyScale);
    }

    public static TransformModel FromLocalTransform(Transform transform)
    {
        return new TransformModel(transform.localPosition, transform.localRotation, transform.localScale);
    }

    public void ApplyToLocalTransform(Transform transform)
    {
        transform.localPosition = Position;
        transform.localRotation = Rotation;
        transform.localScale = Scale;
    }
}
