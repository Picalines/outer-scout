using SceneRecorder.Infrastructure.Extensions;

namespace SceneRecorder.Recording.Animation;

internal sealed class ComposedAnimator : IAnimator
{
    private readonly HashSet<IAnimator> _animators = [];

    public void AddAnimator(IAnimator animator)
    {
        _animators.Add(animator);
    }

    public void ApplyFrame(int frame)
    {
        _animators.ForEach(animator => animator.ApplyFrame(frame));
    }
}
