using SceneRecorder.Shared.Extensions;
using UnityEngine;

namespace SceneRecorder.WebApi.DTOs.Json;

internal sealed class Vector2Converter : NumberArrayConverter<Vector2>
{
    public Vector2Converter()
        : base(2) { }

    protected override Vector2 ReadJson(ReadOnlySpan<float> array)
    {
        return new Vector2(array[0], array[1]);
    }

    protected override void WriteJson(in Vector2 value, ref Span<float> array)
    {
        (array[0], array[1]) = value;
    }
}
