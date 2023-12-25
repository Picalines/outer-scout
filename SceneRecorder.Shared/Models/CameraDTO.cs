using Newtonsoft.Json;
using SceneRecorder.Shared.Models.JsonConverters;
using UnityEngine;

namespace SceneRecorder.Shared.Models;

public readonly record struct CameraDTO
{
    [JsonConverter(typeof(Vector2Converter))]
    public required Vector2 SensorSize { get; init; }

    public required float FocalLength { get; init; }

    [JsonConverter(typeof(Vector2Converter))]
    public required Vector2 LensShift { get; init; }

    public required float NearClipPlane { get; init; }

    public required float FarClipPlane { get; init; }

    public static CameraDTO FromOWCamera(OWCamera owCamera)
    {
        var camera = owCamera.mainCamera;

        return new()
        {
            SensorSize = camera.sensorSize,
            FocalLength = camera.focalLength,
            LensShift = camera.lensShift,
            NearClipPlane = owCamera.nearClipPlane,
            FarClipPlane = owCamera.farClipPlane,
        };
    }

    public void Apply(OWCamera owCamera)
    {
        var camera = owCamera.mainCamera;

        camera.usePhysicalProperties = true;
        camera.sensorSize = SensorSize;
        camera.focalLength = FocalLength;
        camera.lensShift = LensShift;

        owCamera.nearClipPlane = NearClipPlane;
        owCamera.farClipPlane = FarClipPlane;
    }
}
