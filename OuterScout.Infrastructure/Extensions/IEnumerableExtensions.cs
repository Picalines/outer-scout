namespace OuterScout.Infrastructure.Extensions;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
        {
            action(item);
            yield return item;
        }
    }

    public static IEnumerable<(int Index, T Value)> Indexed<T>(this IEnumerable<T> enumerable)
    {
        int index = 0;
        foreach (var value in enumerable)
        {
            yield return (index++, value);
        }
    }

    public static IEnumerable<(T Item, bool IsLast)> WithIsLast<T>(this IReadOnlyList<T> list)
    {
        int index = 0;
        var lastIndex = list.Count - 1;

        foreach (var element in list)
        {
            yield return (element, index++ == lastIndex);
        }
    }

    public static Dictionary<K, V> ToDictionary<K, V>(
        this IEnumerable<KeyValuePair<K, V>> dictonary
    )
    {
        return dictonary.ToDictionary(p => p.Key, p => p.Value);
    }

    public static V GetOrCreate<K, V>(this IDictionary<K, V> dictionary, K key)
        where V : new()
    {
        if (dictionary.TryGetValue(key, out var value) is false)
        {
            value = dictionary[key] = new();
        }

        return value;
    }

    public static V GetOrCreate<K, V>(this IDictionary<K, V> dictionary, K key, Func<V> create)
    {
        if (dictionary.TryGetValue(key, out var value) is false)
        {
            value = dictionary[key] = create();
        }

        return value;
    }
}
