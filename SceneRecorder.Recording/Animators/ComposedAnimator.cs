namespace Picalines.OuterWilds.SceneRecorder.Recording.Animators;

internal sealed class ComposedAnimator : IAnimator
{
    public IReadOnlyList<IAnimator> Animators { get; set; } = Array.Empty<IAnimator>();

    public IAnimator? MainAnimator
    {
        get => Animators.FirstOrDefault();
    }

    public int FrameCount
    {
        get => MainAnimator?.FrameCount ?? 0;
        set
        {
            foreach (var animator in Animators)
            {
                animator.FrameCount = value;
            }
        }
    }

    public void SetFrame(int frameIndex)
    {
        foreach (var animator in Animators)
        {
            animator.SetFrame(frameIndex);
        }
    }
}
