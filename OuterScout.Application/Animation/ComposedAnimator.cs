using System.Collections;
using OuterScout.Domain;

namespace OuterScout.Application.Animation;

internal sealed class ComposedAnimator : IAnimator
{
    private readonly IReadOnlyList<IAnimator> _animators;

    public ComposedAnimator(IReadOnlyList<IAnimator> animators)
    {
        _animators = animators;
    }

    public IEnumerator ApplyFrames(IntRange frameRange)
    {
        var appliers = _animators.Select(animator => animator.ApplyFrames(frameRange)).ToArray();

        foreach (var _ in frameRange)
        {
            foreach (var applier in appliers)
            {
                applier.MoveNext();
            }

            yield return null;
        }
    }
}
