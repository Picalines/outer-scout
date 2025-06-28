using OuterScout.Shared.Extensions;

namespace OuterScout.Application.Animation;

internal sealed class ComposedAnimator
{
    private readonly IReadOnlyList<PropertyAnimator> _animators;

    public ComposedAnimator(IReadOnlyList<PropertyAnimator> animators)
    {
        _animators = animators;
    }

    public void ApplyFrame(int frame)
    {
        _animators.ForEach(animator => animator.ApplyFrame(frame));
    }
}
