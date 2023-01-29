using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Extensions;

public static class TransformExtensions
{
    public static void CopyGlobalTransformTo(this Transform sourceTransform, Transform destinationTransform)
    {
        destinationTransform.position = sourceTransform.position;
        destinationTransform.rotation = sourceTransform.rotation;
        destinationTransform.localScale = sourceTransform.localScale;
    }
}
