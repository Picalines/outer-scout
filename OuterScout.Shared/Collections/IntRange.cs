using System.Collections;
using OuterScout.Shared.Validation;

namespace OuterScout.Shared.Collections;

public record struct IntRange : IEnumerable<int>
{
    public int Start { get; }

    public int End { get; }

    public IntRange()
    {
        Start = 0;
        End = 0;
    }

    private IntRange(int start, int end)
    {
        end.Throw().IfLessThan(start);

        Start = start;
        End = end;
    }

    public static IntRange FromValues(int startValue, int endValue)
    {
        return new IntRange(startValue, endValue);
    }

    public static IntRange FromOffset(int startValue, int offset)
    {
        offset.Throw().IfLessThan(0);

        return FromValues(startValue, startValue + offset);
    }

    public static IntRange FromCount(int startValue, int valuesCount)
    {
        valuesCount.Throw().IfLessThan(1);

        return FromValues(startValue, startValue + valuesCount - 1);
    }

    public int Length
    {
        get => End - Start;
    }

    public bool Contains(int value)
    {
        return value >= Start && value <= End;
    }

    public bool Contains(IntRange innerRange)
    {
        return Contains(innerRange.Start) && Contains(innerRange.End);
    }

    public int ValueToIndex(int valueInRange)
    {
        valueInRange.Throw().If(!Contains(valueInRange));

        return valueInRange - Start;
    }

    public int IndexToValue(int index)
    {
        index.Throw().IfLessThan(0);

        return (Start + index).Throw().IfGreaterThan(End).OrReturn();
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
