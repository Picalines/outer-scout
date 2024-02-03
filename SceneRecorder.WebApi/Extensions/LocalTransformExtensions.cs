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
            Position = transformDTO.Position,
            Rotation = transformDTO.Rotation,
            Scale = transformDTO.Scale,
            Parent = parent
        };
    }
}
