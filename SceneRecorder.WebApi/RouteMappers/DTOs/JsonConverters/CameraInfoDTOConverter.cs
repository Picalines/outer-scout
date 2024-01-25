using SceneRecorder.Shared.DTOs;
using SceneRecorder.Shared.Extensions;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteMappers.DTOs.JsonConverters;

internal sealed class CameraInfoDTOConverter : NumberArrayConverter<CameraInfoDTO>
{
    public CameraInfoDTOConverter()
        : base(8) { }

    protected override CameraInfoDTO ReadJson(ReadOnlySpan<float> array)
    {
        return new()
        {
            FocalLength = array[0],
            SensorSize = new Vector2(array[1], array[2]),
            LensShift = new Vector2(array[3], array[4]),
            NearClipPlane = array[5],
            FarClipPlane = array[6],
            GateFit = (Camera.GateFitMode)(int)array[7],
        };
    }

    protected override void WriteJson(in CameraInfoDTO value, ref Span<float> array)
    {
        array[0] = value.FocalLength;

        (array[1], array[2]) = value.SensorSize;
        (array[3], array[4]) = value.LensShift;

        array[5] = value.NearClipPlane;
        array[6] = value.FarClipPlane;

        array[7] = (int)value.GateFit;
    }
}
