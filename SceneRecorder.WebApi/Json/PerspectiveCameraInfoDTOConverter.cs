using SceneRecorder.Infrastructure.Extensions;
using UnityEngine;

namespace SceneRecorder.WebApi.DTOs.Json;

internal sealed class PerspectiveCameraInfoDTOConverter
    : NumberArrayConverter<PerspectiveCameraInfoDTO>
{
    public PerspectiveCameraInfoDTOConverter()
        : base(7) { }

    protected override PerspectiveCameraInfoDTO ReadJson(ReadOnlySpan<float> array)
    {
        return new()
        {
            FocalLength = array[0],
            SensorSize = new Vector2(array[1], array[2]),
            LensShift = new Vector2(array[3], array[4]),
            NearClipPlane = array[5],
            FarClipPlane = array[6],
        };
    }

    protected override void WriteJson(in PerspectiveCameraInfoDTO value, ref Span<float> array)
    {
        array[0] = value.FocalLength;

        (array[1], array[2]) = value.SensorSize;
        (array[3], array[4]) = value.LensShift;

        array[5] = value.NearClipPlane;
        array[6] = value.FarClipPlane;
    }
}
