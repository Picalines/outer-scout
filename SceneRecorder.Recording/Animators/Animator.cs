namespace SceneRecorder.Recording.Animators;

internal abstract class Animator<T> : IAnimator<T>
{
    private readonly List<T> _ValuesAtFrames = new();

    private readonly T _DefaultFrameValue;

    public Animator(T? defaultValue)
    {
        _DefaultFrameValue = defaultValue!;
    }

    public int StartFrame { get; private set; } = 0;

    public int EndFrame { get; private set; } = 0;

    public void SetFrameRange(int startFrame, int endFrame)
    {
        if (startFrame > endFrame)
        {
            throw new ArgumentOutOfRangeException();
        }

        StartFrame = startFrame;
        EndFrame = endFrame;

        ResizeList(_ValuesAtFrames, this.GetFrameCount(), _DefaultFrameValue);
    }

    public void SetValueAtFrame(int frame, in T value)
    {
        AssertFrameInRange(frame);
        _ValuesAtFrames[FrameNumberToIndex(frame)] = value;
    }

    public void SetFrame(int frame)
    {
        AssertFrameInRange(frame);
        ApplyValue(_ValuesAtFrames[FrameNumberToIndex(frame)]);
    }

    protected abstract void ApplyValue(T value);

    private int FrameNumberToIndex(int frame)
    {
        return frame - StartFrame;
    }

    private void AssertFrameInRange(int frame)
    {
        if (frame < StartFrame || frame > EndFrame)
        {
            throw new ArgumentOutOfRangeException(nameof(frame));
        }
    }

    private static void ResizeList<TValue>(List<TValue> list, int newSize, TValue newItemValue)
    {
        var currentSize = list.Count;

        if (currentSize == newSize)
        {
            return;
        }

        if (newSize < currentSize)
        {
            list.RemoveRange(newSize, currentSize - newSize);
        }
        else
        {
            if (newSize > list.Capacity)
                list.Capacity = newSize;

            list.AddRange(Enumerable.Repeat(newItemValue, newSize - currentSize));
        }
    }
}
