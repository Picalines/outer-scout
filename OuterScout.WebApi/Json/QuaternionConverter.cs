using OuterScout.Infrastructure.Extensions;
using UnityEngine;

namespace OuterScout.WebApi.Json;

internal sealed class QuaternionConverter : NumberArrayConverter<Quaternion>
{
    public QuaternionConverter()
        : base(4) { }

    protected override Quaternion ReadJson(ReadOnlySpan<float> array)
    {
        return new Quaternion(array[0], array[1], array[2], array[3]);
    }

    protected override void WriteJson(in Quaternion value, ref Span<float> array)
    {
        (array[0], array[1], array[2], array[3]) = value;
    }
}
