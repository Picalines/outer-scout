using SceneRecorder.Domain;
using SceneRecorder.WebApi.DTOs;
using UnityEngine;

namespace SceneRecorder.WebApi.Extensions;

internal static class LocalTransformExtensions
{
    public static LocalTransform ToLocalTransform(this TransformDTO transformDTO, Transform? parent)
    {
        return new LocalTransform()
        {
            Position = transformDTO.Position ?? Vector3.zero,
            Rotation = transformDTO.Rotation ?? Quaternion.identity,
            Scale = transformDTO.Scale ?? Vector3.one,
            Parent = parent
        };
    }
}
