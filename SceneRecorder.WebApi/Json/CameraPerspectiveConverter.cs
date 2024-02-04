using SceneRecorder.Infrastructure.Extensions;
using UnityEngine;

namespace SceneRecorder.WebApi.DTOs.Json;

internal sealed class CameraPerspectiveConverter : NumberArrayConverter<CameraPerspectiveDTO>
{
    public CameraPerspectiveConverter()
        : base(7) { }

    protected override CameraPerspectiveDTO ReadJson(ReadOnlySpan<float> array)
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

    protected override void WriteJson(in CameraPerspectiveDTO value, ref Span<float> array)
    {
        array[0] = value.FocalLength;

        (array[1], array[2]) = value.SensorSize;
        (array[3], array[4]) = value.LensShift;

        array[5] = value.NearClipPlane;
        array[6] = value.FarClipPlane;
    }
}
