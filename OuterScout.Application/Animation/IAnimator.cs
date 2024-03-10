using System.Collections;
using OuterScout.Domain;

namespace OuterScout.Application.Animation;

public interface IAnimator
{
    public IEnumerator ApplyFrames(IntRange frameRange);
}
