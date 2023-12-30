using System.Collections;

namespace SceneRecorder.Recording.Animators;

internal abstract class Animator<T> : IAnimator<T>
{
    private readonly List<T> _ValuesAtFrames = new();

    private readonly BitArray _HasFrames = new(0, false);

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
        var frameCount = this.GetFrameCount();

        ResizeList(_ValuesAtFrames, frameCount, _DefaultFrameValue);
        _HasFrames.Length = frameCount;
    }

    public void SetValueAtFrame(int frame, in T value)
    {
        AssertFrameInRange(frame);

        var frameIndex = FrameNumberToIndex(frame);

        _ValuesAtFrames[frameIndex] = value;
        _HasFrames.Set(frameIndex, true);
    }

    public void SetFrame(int frame)
    {
        AssertFrameInRange(frame);

        var frameIndex = FrameNumberToIndex(frame);

        if (_HasFrames[frameIndex])
        {
            ApplyValue(_ValuesAtFrames[frameIndex]);
        }
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
            {
                list.Capacity = newSize;
            }

            list.AddRange(Enumerable.Repeat(newItemValue, newSize - currentSize));
        }
    }
}
