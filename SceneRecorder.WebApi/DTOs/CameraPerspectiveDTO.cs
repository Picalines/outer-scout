using SceneRecorder.Domain;
using UnityEngine;

namespace SceneRecorder.WebApi.DTOs;

internal record struct CameraPerspectiveDTO
{
    public required float FocalLength { get; init; }

    public required Vector2 SensorSize { get; init; }

    public required Vector2 LensShift { get; init; }

    public required float NearClipPlane { get; init; }

    public required float FarClipPlane { get; init; }

    public CameraPerspective ToPerspective()
    {
        return new()
        {
            FocalLength = FocalLength,
            SensorSize = SensorSize,
            LensShift = LensShift,
            NearClipPlane = NearClipPlane,
            FarClipPlane = FarClipPlane,
        };
    }
}
