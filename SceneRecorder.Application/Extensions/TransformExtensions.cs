using System.Runtime.CompilerServices;
using SceneRecorder.Domain;
using UnityEngine;

namespace SceneRecorder.Application.Extensions;

public static class TransformExtensions
{
    private static readonly ConditionalWeakTable<Transform, string> _CachedTransformPaths = new();

    public static void Apply(this Transform transform, LocalTransform localTransform)
    {
        var (position, rotation, scale, parent) = localTransform;

        if (parent is null)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;
        }
        else
        {
            transform.position = parent.TransformPoint(position);
            transform.rotation = parent.rotation * rotation;
            transform.localScale = scale;
        }
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
