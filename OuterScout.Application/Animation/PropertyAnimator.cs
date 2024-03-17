using System.Collections;
using OuterScout.Domain;

namespace OuterScout.Application.Animation;

public sealed class PropertyAnimator
{
    public PropertyCurve Curve { get; }

    public PropertyApplier Applier { get; }

    public PropertyAnimator(PropertyCurve curve, PropertyApplier applier)
    {
        Curve = curve;
        Applier = applier;
    }

    public IEnumerator ApplyFrames(IntRange frameRange)
    {
        if (Curve.IsEmpty)
        {
            return Array.Empty<object>().GetEnumerator();
        }

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
