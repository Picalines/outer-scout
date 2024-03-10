using OuterScout.Domain;
using OuterScout.Infrastructure.Validation;

namespace OuterScout.Application.Animation;

public sealed class PropertyCurve<T> : IPropertyCurve<T>
{
    private readonly Lerper<T> _lerper;

    private readonly SortedDictionary<int, Keyframe<T>> _keyframes = [];

    public PropertyCurve(Lerper<T> lerper)
    {
        _lerper = lerper;
    }

    public void StoreKeyframe(Keyframe<T> keyframe)
    {
        // SortedDictionary.Enumerator will throw InvalidOperationException
        // if a Curve is modified during iteration

        _keyframes[keyframe.Frame] = keyframe;
    }

    public IEnumerable<T> GetValues(IntRange frameRange)
    {
        _keyframes.Count.Throw().IfEquals(0);

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

            var range = IntRange.FromValues(leftKeyframe.Frame, rightKeyframe.Frame);
            var progress = ((float)currentFrame - range.Start) / range.Length;

            yield return _lerper(leftKeyframe.Value, rightKeyframe.Value, progress);

            currentFrame++;
        }

        var lastKeyframe = rightKeyframe;

        while (currentFrame++ <= endFrame)
        {
            yield return lastKeyframe.Value;
        }
    }
}
