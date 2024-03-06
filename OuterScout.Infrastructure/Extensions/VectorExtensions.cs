using UnityEngine;

namespace OuterScout.Infrastructure.Extensions;

public static class VectorExtensions
{
    public static void Deconstruct(this Vector2 vector, out float x, out float y)
    {
        x = vector.x;
        y = vector.y;
    }

    public static void Deconstruct(this Vector3 vector, out float x, out float y, out float z)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public static void Deconstruct(
        this Quaternion quaternion,
        out float x,
        out float y,
        out float z,
        out float w
    )
    {
        x = quaternion.x;
        y = quaternion.y;
        z = quaternion.z;
        w = quaternion.w;
    }
}
