using System.Collections;
using SceneRecorder.Shared.Validation;

namespace SceneRecorder.Shared;

public record struct IntRange : IEnumerable<int>
{
    public int Start { get; }

    public int End { get; }

    public IntRange()
    {
        Start = 0;
        End = 0;
    }

    public IntRange(int start, int end)
    {
        end.Throw().IfLessThan(start);

        Start = start;
        End = end;
    }

    public int Length
    {
        get => End - Start;
    }

    public IEnumerator<int> GetEnumerator()
    {
        return Enumerable.Range(Start, Length + 1).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
