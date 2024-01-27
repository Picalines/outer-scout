using SceneRecorder.Recording.Domain;
using UnityEngine;

namespace SceneRecorder.Recording.Animation.ValueApplication;

public sealed class LocalTransformApplier : IValueApplier<LocalTransform>
{
    public required Transform TargetTransform { get; init; }

    public void Apply(LocalTransform localTransform)
    {
        var (position, rotation, scale, parent) = localTransform;

        if (parent is null)
        {
            TargetTransform.position = position;
            TargetTransform.rotation = rotation;
            TargetTransform.localScale = scale;
            return;
        }

        TargetTransform.position = parent.TransformPoint(position);
        TargetTransform.rotation = parent.rotation * rotation;
        TargetTransform.localScale = scale;
    }
}
