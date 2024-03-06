using OuterScout.Infrastructure.Extensions;

namespace OuterScout.Application.Animation;

internal sealed class ComposedAnimator : IAnimator
{
    private readonly IReadOnlyList<IAnimator> _animators;

    public ComposedAnimator(IReadOnlyList<IAnimator> animators)
    {
        _animators = animators;
    }

    public void ApplyFrame(int frame)
    {
        _animators.ForEach(animator => animator.ApplyFrame(frame));
    }
}
