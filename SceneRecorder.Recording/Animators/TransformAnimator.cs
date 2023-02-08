using Picalines.OuterWilds.SceneRecorder.Shared.Models;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording.Animators;

internal sealed class TransformAnimator : Animator<TransformModel>
{
    public enum TransformApplyMode
    {
        Local,
        GlobalPositionAndRotation,
    }

    public Transform TargetTransform { get; }

    public TransformApplyMode ApplyMode { get; init; } = TransformApplyMode.Local;

    public TransformAnimator(Transform targetTransform)
    {
        TargetTransform = targetTransform;
    }

    protected override void ApplyValue(TransformModel currentTransform)
    {
        switch (ApplyMode)
        {
            case TransformApplyMode.Local:
                currentTransform.ApplyToLocalTransform(TargetTransform);
                break;

            case TransformApplyMode.GlobalPositionAndRotation:
                currentTransform.ApplyToGlobalPositionAndRotation(TargetTransform);
                break;

            default:
                throw new NotImplementedException();
        }
    }
}
