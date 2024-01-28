namespace SceneRecorder.Infrastructure.Extensions;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
        {
            action(item);
        }

        return enumerable;
    }

    public static IEnumerable<(int Index, T Value)> Indexed<T>(this IEnumerable<T> enumerable)
    {
        int index = 0;
        foreach (var value in enumerable)
        {
            yield return (index++, value);
        }
    }
}
