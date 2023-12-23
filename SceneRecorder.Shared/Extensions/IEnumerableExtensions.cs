namespace Picalines.OuterWilds.SceneRecorder.Shared.Extensions;

public static class IEnumerableExtensions
{
    public static TSource? MinByOrDefault<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> selector
    )
    {
        return source.MinByOrDefault(selector, Comparer<TKey>.Default);
    }

    public static TSource? MinByOrDefault<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> selector,
        IComparer<TKey> comparer
    )
    {
        if (source is null)
            throw new ArgumentNullException("source");
        if (selector is null)
            throw new ArgumentNullException("selector");

        using var sourceIterator = source.GetEnumerator();

        if (!sourceIterator.MoveNext())
        {
            return default;
        }

        var min = sourceIterator.Current;
        var selectedMin = selector(min);

        while (sourceIterator.MoveNext())
        {
            var element = sourceIterator.Current;
            var selectedElement = selector(element);
            if (comparer.Compare(selectedElement, selectedMin) < 0)
            {
                min = element;
                selectedMin = selectedElement;
            }
        }

        return min;
    }
}
