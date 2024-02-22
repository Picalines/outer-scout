using SceneRecorder.Application.Animation.Interpolation;
using SceneRecorder.Application.Animation.ValueApplication;
using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.Application.Animation;

public sealed class Animator<T> : IAnimator
{
    public required KeyframeStorage<T> Keyframes { get; init; }

    public required IInterpolation<T> Interpolation { get; init; }

    public required IValueApplier<T> ValueApplier { get; init; }

    public void ApplyFrame(int frame)
    {
        this.Throw().If(Keyframes.IsEmpty);
        frame.Throw().If(!Keyframes.FrameRange.Contains(frame));

        var (range, left, right) = Keyframes.GetRightSpan(frame).GetValueOrDefault();

        var progress = range.Length is 0 ? 1f : ((float)frame - range.Start) / range.Length;

        var valueToApply = Interpolation.Interpolate(left, right, progress);

        ValueApplier.Apply(valueToApply);
    }
}
