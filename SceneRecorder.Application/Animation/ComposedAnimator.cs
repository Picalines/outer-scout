using SceneRecorder.Infrastructure.Extensions;

namespace SceneRecorder.Application.Animation;

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
