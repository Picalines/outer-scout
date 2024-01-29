using SceneRecorder.Domain;
using SceneRecorder.WebApi.DTOs;

namespace SceneRecorder.WebApi.Extensions;

internal static class CameraInfoExtensions
{
    public static PerspectiveCameraInfoDTO ToDTO(this PerspectiveCameraInfo cameraInfo)
    {
        return new PerspectiveCameraInfoDTO()
        {
            FocalLength = cameraInfo.FocalLength,
            SensorSize = cameraInfo.SensorSize,
            LensShift = cameraInfo.LensShift,
            NearClipPlane = cameraInfo.NearClipPlane,
            FarClipPlane = cameraInfo.FarClipPlane,
            GateFit = cameraInfo.GateFit,
        };
    }
}
