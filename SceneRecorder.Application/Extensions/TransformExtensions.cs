using System.Runtime.CompilerServices;
using SceneRecorder.Domain;
using UnityEngine;

namespace SceneRecorder.Application.Extensions;

public static class TransformExtensions
{
    private static readonly ConditionalWeakTable<Transform, string> _pathCache = new();

    public static void ResetLocal(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

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

    public static void ApplyWithParent(this Transform transform, LocalTransform localTransform)
    {
        transform.parent = localTransform.Parent;
        transform.Apply(localTransform);
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
