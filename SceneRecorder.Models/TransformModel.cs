#if IS_TARGET_MOD

using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.Models.JsonConverters;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Models;

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
}

#endif