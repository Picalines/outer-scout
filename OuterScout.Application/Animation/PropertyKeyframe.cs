using OuterScout.Domain;
using UnityEngine;

namespace OuterScout.Application.Animation;

public readonly record struct PropertyKeyframe(int Frame, float Value)
{
    public float Interpolate(PropertyKeyframe nextKeyframe, int frame)
    {
        if (nextKeyframe.Frame < Frame)
        {
            return nextKeyframe.Interpolate(this, frame);
        }

        var range = IntRange.FromValues(Frame, nextKeyframe.Frame);
        var t = ((float)frame - range.Start) / range.Length;

        return Mathf.Lerp(Value, nextKeyframe.Value, t);
    }
}
