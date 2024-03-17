using System.Collections;
using OuterScout.Domain;

namespace OuterScout.Application.Animation;

internal sealed class ComposedAnimator
{
    private readonly IReadOnlyList<PropertyAnimator> _animators;

    public ComposedAnimator(IReadOnlyList<PropertyAnimator> animators)
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
