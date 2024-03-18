using System.Runtime.CompilerServices;
using UnityEngine;

namespace OuterScout.Application.Extensions;

public static class TransformExtensions
{
    private static readonly ConditionalWeakTable<Transform, string> _pathCache = new();

    public static void ResetLocal(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public static string GetPath(this Transform transform)
    {
        if (_pathCache.TryGetValue(transform, out var path) is true)
        {
            return path;
        }

        path =
            transform.parent == null
                ? transform.name
                : $"{GetPath(transform.parent)}/{transform.name}";

        _pathCache.Add(transform, path);

        return path;
    }
}
