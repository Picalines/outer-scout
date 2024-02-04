using SceneRecorder.Domain;
using SceneRecorder.WebApi.DTOs;

namespace SceneRecorder.WebApi.Extensions;

internal static class CameraInfoExtensions
{
    public static CameraPerspectiveDTO ToDTO(this CameraPerspective cameraInfo)
    {
        return new CameraPerspectiveDTO()
        {
            FocalLength = cameraInfo.FocalLength,
            SensorSize = cameraInfo.SensorSize,
            LensShift = cameraInfo.LensShift,
            NearClipPlane = cameraInfo.NearClipPlane,
            FarClipPlane = cameraInfo.FarClipPlane,
        };
    }
}
