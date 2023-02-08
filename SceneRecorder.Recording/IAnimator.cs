namespace Picalines.OuterWilds.SceneRecorder.Recording;

public interface IAnimator
{
    public int FrameCount { get; set; }

    public void SetFrame(int frameIndex);
}

public interface IAnimator<T> : IAnimator
{
    public void SetValueAtFrame(int frameIndex, T value);
}
