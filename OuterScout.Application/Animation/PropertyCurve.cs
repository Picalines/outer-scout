using OuterScout.Domain;
using OuterScout.Infrastructure.Validation;

namespace OuterScout.Application.Animation;

public sealed class PropertyCurve
{
    private readonly SortedDictionary<int, PropertyKeyframe> _keyframes = [];

    public bool IsEmpty
    {
        get => _keyframes.Count is 0;
    }

    public void StoreKeyframe(PropertyKeyframe keyframe)
    {
        // SortedDictionary.Enumerator will throw InvalidOperationException
        // if a Curve is modified during iteration

        _keyframes[keyframe.Frame] = keyframe;
    }

    public IEnumerable<float> GetValues(IntRange frameRange)
    {
        IsEmpty.Throw().IfTrue();

        if (_keyframes.Count is 1)
        {
            yield return _keyframes.Values.First().Value;
            yield break;
        }

        var (currentFrame, endFrame) = (frameRange.Start, frameRange.End);

        using var keyframeEnumerator = _keyframes.Values.GetEnumerator();
        keyframeEnumerator.MoveNext();

        var firstKeyframe = keyframeEnumerator.Current;
        while (currentFrame <= endFrame && currentFrame < firstKeyframe.Frame)
        {
            currentFrame++;
            yield return firstKeyframe.Value;
        }

        keyframeEnumerator.MoveNext();
        var leftKeyframe = firstKeyframe;
        var rightKeyframe = keyframeEnumerator.Current;

        while (currentFrame <= endFrame)
        {
            while (currentFrame >= rightKeyframe.Frame)
            {
                if (keyframeEnumerator.MoveNext() is false)
                {
                    break;
                }

                leftKeyframe = rightKeyframe;
                rightKeyframe = keyframeEnumerator.Current;
            }

            yield return leftKeyframe.Interpolate(rightKeyframe, currentFrame);

            currentFrame++;
        }

        var lastKeyframe = rightKeyframe;

        while (currentFrame++ <= endFrame)
        {
            yield return lastKeyframe.Value;
        }
    }
}
