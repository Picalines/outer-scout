namespace Picalines.OuterWilds.SceneRecorder.WebApi;

internal static class UnityObjectExtensions
{
    public static T? Nullable<T>(this T? @object)
        where T : UnityEngine.Object
    {
        return @object != null // Unity objects have an equality overload!
            ? @object
            : null;
    }
}
