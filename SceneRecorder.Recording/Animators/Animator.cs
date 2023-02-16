namespace Picalines.OuterWilds.SceneRecorder.Recording.Animators;

internal abstract class Animator<T> : IAnimator<T>
{
    private readonly List<T> _ValuesAtFrames = new();

    private readonly T _DefaultFrameValue;

    public Animator(T? defaultValue)
    {
        _DefaultFrameValue = defaultValue!;
    }

    public int FrameCount
    {
        get => _ValuesAtFrames.Count;
        set => ResizeList(_ValuesAtFrames, value, _DefaultFrameValue);
    }

    public void SetValueAtFrame(int frameIndex, in T value)
    {
        _ValuesAtFrames[frameIndex] = value;
    }

    public void SetFrame(int frameIndex)
    {
        ApplyValue(_ValuesAtFrames[frameIndex]);
    }

    protected abstract void ApplyValue(T value);

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
