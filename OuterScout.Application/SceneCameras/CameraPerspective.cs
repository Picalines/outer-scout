using UnityEngine;

namespace OuterScout.Application.SceneCameras;

public record struct CameraPerspective
{
    public required float FocalLength { get; init; }

    public required Vector2 SensorSize { get; init; }

    public required Vector2 LensShift { get; init; }

    public required float NearClipPlane { get; init; }

    public required float FarClipPlane { get; init; }
}
