using System.Collections;
using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.Domain;

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

    public bool Contains(int value)
    {
        return value >= Start && value <= End;
    }

    public int ValueToIndex(int valueInRange)
    {
        valueInRange.Throw().If(!Contains(valueInRange));

        return valueInRange - Start;
    }

    public int IndexToValue(int index)
    {
        index.Throw().IfLessThan(0);

        var value = Start + index;
        value.Throw().IfGreaterThan(End);

        return value;
    }

    public void Deconstruct(out int start, out int end)
    {
        start = Start;
        end = End;
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
