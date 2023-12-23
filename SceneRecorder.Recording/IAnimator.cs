namespace SceneRecorder.Recording;

public interface IAnimator
{
    public int StartFrame { get; }

    public int EndFrame { get; }

    public void SetFrameRange(int startFrame, int endFrame);

    public void SetFrame(int frame);
}

public interface IAnimator<T> : IAnimator
{
    public void SetValueAtFrame(int frame, in T value);
}
