namespace Picalines.OuterWilds.SceneRecorder.Recording.Animators;

internal sealed class ComposedAnimator : IAnimator
{
    public IReadOnlyList<IAnimator> Animators { get; set; } = Array.Empty<IAnimator>();

    public IAnimator? MainAnimator
    {
        get => Animators.FirstOrDefault();
    }

    public int StartFrame
    {
        get => MainAnimator?.StartFrame ?? 0;
    }

    public int EndFrame
    {
        get => MainAnimator?.EndFrame ?? 0;
    }

    public void SetFrameRange(int startFrame, int endFrame)
    {
        foreach (var animator in Animators)
        {
            animator.SetFrameRange(startFrame, endFrame);
        }
    }

    public void SetFrame(int frame)
    {
        foreach (var animator in Animators)
        {
            animator.SetFrame(frame);
        }
    }
}
