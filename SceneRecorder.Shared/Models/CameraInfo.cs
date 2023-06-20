using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.Shared.Models.JsonConverters;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Models;

public readonly record struct CameraInfo
{
    [JsonProperty("sensor_size")]
    [JsonConverter(typeof(Vector2Converter))]
    public required Vector2 SensorSize { get; init; }

    [JsonProperty("focal_length")]
    public required float FocalLength { get; init; }

    [JsonProperty("lens_shift")]
    [JsonConverter(typeof(Vector2Converter))]
    public required Vector2 LensShift { get; init; }

    [JsonProperty("near_clip_plane")]
    public required float NearClipPlane { get; init; }

    [JsonProperty("far_clip_plane")]
    public required float FarClipPlane { get; init; }

    public static CameraInfo FromOWCamera(OWCamera owCamera)
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

    public void ApplyToOWCamera(OWCamera owCamera)
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
