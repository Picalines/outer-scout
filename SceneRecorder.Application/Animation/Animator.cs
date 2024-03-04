using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.Application.Animation;

public delegate T Interpolation<T>(T left, T right, float progress);

public delegate void ValueApplier<T>(T value);

public sealed class Animator<T> : IAnimator
{
    public required KeyframeStorage<T> Keyframes { get; init; }

    public required Interpolation<T> Interpolation { get; init; }

    public required ValueApplier<T> ValueApplier { get; init; }

    public void ApplyFrame(int frame)
    {
        frame.Throw().If(!Keyframes.FrameRange.Contains(frame));

        if (Keyframes.IsEmpty)
        {
            return;
        }

        var (range, left, right) = Keyframes.GetRightSpan(frame).GetValueOrDefault();

        var progress = range.Length is 0 ? 1f : ((float)frame - range.Start) / range.Length;

        var valueToApply = Interpolation(left, right, progress);

        ValueApplier(valueToApply);
    }
}
