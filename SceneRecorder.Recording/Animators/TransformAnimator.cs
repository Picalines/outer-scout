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

    public TransformApplyMode ApplyMode { get; }

    public TransformAnimator(Transform targetTransform, TransformApplyMode applyMode = TransformApplyMode.Local)
        : base(GetDefaultFrameValue(targetTransform, applyMode))
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

    private static TransformModel GetDefaultFrameValue(Transform targetTransform, TransformApplyMode applyMode)
    {
        return applyMode switch
        {
            TransformApplyMode.Local => TransformModel.FromLocalTransform(targetTransform),
            TransformApplyMode.GlobalPositionAndRotation => TransformModel.FromGlobalTransform(targetTransform),
            _ => throw new NotImplementedException(),
        };
    }
}
