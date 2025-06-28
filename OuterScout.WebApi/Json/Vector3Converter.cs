using OuterScout.Shared.Extensions;
using UnityEngine;

namespace OuterScout.WebApi.Json;

internal sealed class Vector3Converter : NumberArrayConverter<Vector3>
{
    public Vector3Converter()
        : base(3) { }

    protected override Vector3 ReadJson(ReadOnlySpan<float> array)
    {
        return new Vector3(array[0], array[1], array[2]);
    }

    protected override void WriteJson(in Vector3 value, ref Span<float> array)
    {
        (array[0], array[1], array[2]) = value;
    }
}
