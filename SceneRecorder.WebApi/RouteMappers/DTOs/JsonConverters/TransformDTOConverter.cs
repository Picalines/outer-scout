using SceneRecorder.Shared.Extensions;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteMappers.DTOs.JsonConverters;

internal sealed class TransformDTOConverter : NumberArrayConverter<TransformDTO>
{
    public TransformDTOConverter()
        : base(10) { }

    protected override TransformDTO ReadJson(ReadOnlySpan<float> array)
    {
        return new()
        {
            Position = new Vector3(array[0], array[1], array[2]),
            Rotation = new Quaternion(array[3], array[4], array[5], array[6]),
            Scale = new Vector3(array[7], array[8], array[9]),
        };
    }

    protected override void WriteJson(in TransformDTO value, ref Span<float> array)
    {
        (array[0], array[1], array[2]) = value.Position;
        (array[3], array[4], array[5], array[6]) = value.Rotation;
        (array[7], array[8], array[9]) = value.Scale;
    }
}
