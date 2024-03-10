using System.Collections;
using OuterScout.Domain;

namespace OuterScout.Application.Animation;

public sealed class Animator<T> : IAnimator
{
    public IPropertyCurve<T> Curve { get; }

    public Applier<T> Applier { get; }

    public Animator(IPropertyCurve<T> curve, Applier<T> applier)
    {
        Curve = curve;
        Applier = applier;
    }

    public IEnumerator ApplyFrames(IntRange frameRange)
    {
        return Curve
            .GetValues(frameRange)
            .Select(value =>
            {
                Applier(value);
                return (object?)null;
            })
            .GetEnumerator();
    }
}
