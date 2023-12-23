namespace SceneRecorder.Recording.Animators;

internal sealed class ComposedAnimator<T> : IAnimator<T>
{
    public required IReadOnlyList<IAnimator<T>> Animators { get; init; }

    public IAnimator<T>? MainAnimator
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

    public void SetValueAtFrame(int frame, in T value)
    {
        foreach (var animator in Animators)
        {
            animator.SetValueAtFrame(frame, value);
        }
    }
}
