namespace Picalines.OuterWilds.SceneRecorder.Shared.Extensions;

public static class UnityObjectExtensions
{
    public static T? OrNull<T>(this T? unityObject)
        where T : UnityEngine.Object
    {
#pragma warning disable IDE0029
        return unityObject == null // Unity objects have an equality overload!
            ? null
            : unityObject;
#pragma warning restore IDE0029
    }
}
