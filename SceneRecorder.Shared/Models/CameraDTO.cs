using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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

    [JsonConverter(typeof(StringEnumConverter))]
    public required Camera.GateFitMode GateFit { get; init; }

    public static CameraDTO FromOWCamera(OWCamera owCamera)
    {
        var camera = owCamera.mainCamera;

        if (camera.usePhysicalProperties is false)
        {
            ConvertToPhysicalProperties(camera);
        }

        camera.usePhysicalProperties = true;

        return new()
        {
            SensorSize = camera.sensorSize,
            FocalLength = camera.focalLength,
            LensShift = camera.lensShift,
            NearClipPlane = owCamera.nearClipPlane,
            FarClipPlane = owCamera.farClipPlane,
            GateFit = camera.gateFit,
        };
    }

    public void Apply(OWCamera owCamera)
    {
        var camera = owCamera.mainCamera;
        camera.usePhysicalProperties = true;

        camera.focalLength = FocalLength;
        camera.sensorSize = SensorSize;
        camera.lensShift = LensShift;
        camera.gateFit = GateFit;

        owCamera.nearClipPlane = NearClipPlane;
        owCamera.farClipPlane = FarClipPlane;
    }

    private static void ConvertToPhysicalProperties(Camera camera)
    {
        camera.focalLength = Camera.FieldOfViewToFocalLength(
            camera.fieldOfView,
            sensorSize: camera.gateFit switch
            {
                Camera.GateFitMode.Vertical => camera.sensorSize.x,
                _ => camera.sensorSize.y
            }
        );
    }
}
