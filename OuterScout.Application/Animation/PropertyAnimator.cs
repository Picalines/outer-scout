using UnityEngine;

namespace OuterScout.Application.Animation;

public sealed class PropertyAnimator
{
    public AnimationCurve Curve { get; }

    public PropertyApplier Applier { get; }

    public PropertyAnimator(AnimationCurve curve, PropertyApplier applier)
    {
        Curve = curve;
        Applier = applier;
    }

    public void ApplyFrame(int frame)
    {
        Applier(Curve.Evaluate(frame));
    }
}
