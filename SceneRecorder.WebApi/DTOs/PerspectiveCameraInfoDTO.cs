using SceneRecorder.Domain;
using UnityEngine;

namespace SceneRecorder.WebApi.DTOs;

internal record struct PerspectiveCameraInfoDTO
{
    public required float FocalLength { get; init; }

    public required Vector2 SensorSize { get; init; }

    public required Vector2 LensShift { get; init; }

    public required float NearClipPlane { get; init; }

    public required float FarClipPlane { get; init; }

    public required Camera.GateFitMode GateFit { get; init; }

    public PerspectiveCameraInfo ToCameraInfo()
    {
        return new()
        {
            FocalLength = FocalLength,
            SensorSize = SensorSize,
            LensShift = LensShift,
            NearClipPlane = NearClipPlane,
            FarClipPlane = FarClipPlane,
            GateFit = GateFit,
        };
    }
}
