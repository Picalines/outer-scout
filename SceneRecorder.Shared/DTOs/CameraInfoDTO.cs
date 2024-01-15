using Newtonsoft.Json;
using SceneRecorder.Shared.DTOs.JsonConverters;
using UnityEngine;

namespace SceneRecorder.Shared.DTOs;

[JsonConverter(typeof(CameraInfoDTOConverter))]
public record struct CameraInfoDTO
{
    public float FocalLength { get; set; }

    public Vector2 SensorSize { get; set; }

    public Vector2 LensShift { get; set; }

    public float NearClipPlane { get; set; }

    public float FarClipPlane { get; set; }

    public Camera.GateFitMode GateFit { get; set; }

    public static CameraInfoDTO FromOWCamera(OWCamera owCamera)
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
