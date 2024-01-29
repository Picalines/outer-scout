using UnityEngine;

namespace SceneRecorder.Domain;

public record struct PerspectiveCameraInfo
{
    public required float FocalLength { get; init; }

    public required Vector2 SensorSize { get; init; }

    public required Vector2 LensShift { get; init; }

    public required float NearClipPlane { get; init; }

    public required float FarClipPlane { get; init; }

    public required Camera.GateFitMode GateFit { get; init; }
}
