using SceneRecorder.Shared.Models;
using UnityEngine;

namespace SceneRecorder.Recording.Animators;

internal sealed class TransformAnimator : Animator<TransformDTO>
{
    public enum TransformApplyMode
    {
        Local,
        GlobalPositionAndRotation,
    }

    public Transform TargetTransform { get; }

    public TransformApplyMode ApplyMode { get; }

    public TransformAnimator(
        Transform targetTransform,
        TransformApplyMode applyMode = TransformApplyMode.Local
    )
        : base(GetDefaultFrameValue(targetTransform, applyMode))
    {
        TargetTransform = targetTransform;
    }

    protected override void ApplyValue(TransformDTO currentTransform)
    {
        switch (ApplyMode)
        {
            case TransformApplyMode.Local:
                currentTransform.ApplyLocal(TargetTransform);
                break;

            case TransformApplyMode.GlobalPositionAndRotation:
                currentTransform.ApplyGlobal(TargetTransform);
                break;

            default:
                throw new NotImplementedException();
        }
    }

    private static TransformDTO GetDefaultFrameValue(
        Transform targetTransform,
        TransformApplyMode applyMode
    )
    {
        return applyMode switch
        {
            TransformApplyMode.Local => TransformDTO.FromLocal(targetTransform),
            TransformApplyMode.GlobalPositionAndRotation
                => TransformDTO.FromGlobal(targetTransform),
            _ => throw new NotImplementedException(),
        };
    }
}
