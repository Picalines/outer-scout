using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.Application.Animation;

public delegate void ValueApplier<T>(T value);

public sealed class Animator<T> : IAnimator
{
    public required KeyframeStorage<T> Keyframes { get; init; }

    public required ValueApplier<T> Applier { get; init; }

    public void ApplyFrame(int frame)
    {
        frame.Throw().If(!Keyframes.FrameRange.Contains(frame));

        if (Keyframes.IsEmpty)
        {
            return;
        }

        var (left, right) = Keyframes.GetRightSpan(frame).GetValueOrDefault();

        var range = IntRange.FromValues(left.Frame, right.Frame);
        var progress = range.Length is 0 ? 1f : ((float)frame - range.Start) / range.Length;

        var valueToApply = left.Interpolation(left, right, progress);

        Applier(valueToApply);
    }
}
