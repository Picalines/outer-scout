using System.Runtime.CompilerServices;
using UnityEngine;

namespace SceneRecorder.Infrastructure.Extensions;

public static class TransformExtensions
{
    private static readonly ConditionalWeakTable<Transform, string> _CachedTransformPaths = new();

    public static void CopyGlobalTransformTo(
        this Transform sourceTransform,
        Transform destinationTransform
    )
    {
        destinationTransform.position = sourceTransform.position;
        destinationTransform.rotation = sourceTransform.rotation;
        destinationTransform.localScale = sourceTransform.localScale;
    }

    public static string GetPath(this Transform transform)
    {
        if (_CachedTransformPaths.TryGetValue(transform, out var path) is true)
        {
            return path;
        }

        path =
            transform.parent == null
                ? transform.name
                : $"{GetPath(transform.parent)}/{transform.name}";

        _CachedTransformPaths.Add(transform, path);

        return path;
    }
}
