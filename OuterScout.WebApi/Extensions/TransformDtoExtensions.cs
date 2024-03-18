using OuterScout.WebApi.DTOs;
using UnityEngine;

namespace OuterScout.WebApi.Extensions;

internal static class TransformDtoExtensions
{
    public static void ApplyLocal(this TransformDto transformDto, Transform transform)
    {
        if (transformDto.Position is { } localPosition)
        {
            transform.localPosition = localPosition;
        }

        if (transformDto.Rotation is { } localRotation)
        {
            transform.localRotation = localRotation;
        }

        if (transformDto.Scale is { } localScale)
        {
            transform.localScale = localScale;
        }
    }

    public static void ApplyGlobal(
        this TransformDto transformDto,
        Transform transform,
        Transform parent
    )
    {
        if (transformDto.Position is { } localPosition)
        {
            transform.position = parent.TransformPoint(localPosition);
        }

        if (transformDto.Rotation is { } localRotation)
        {
            transform.rotation = parent.rotation * localRotation;
        }

        if (transformDto.Scale is { } localScale)
        {
            transform.localScale = localScale;
        }
    }

    public static TransformDto GlobalDto(this Transform transform)
    {
        return new TransformDto()
        {
            Position = transform.position,
            Rotation = transform.rotation,
            Scale = transform.lossyScale,
        };
    }

    public static TransformDto InverseDto(this Transform parent, Transform child)
    {
        return new TransformDto()
        {
            Position = parent.InverseTransformPoint(child.position),
            Rotation = parent.InverseTransformRotation(child.rotation),
            Scale = parent.lossyScale,
        };
    }
}
