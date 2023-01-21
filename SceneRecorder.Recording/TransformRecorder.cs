using System.Runtime.InteropServices;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording;

public sealed class TransformRecorder : ValueRecorder<TransformRecorder.TransformData>
{
    [StructLayout(LayoutKind.Auto)]
    public record struct TransformData(
        Vector3 Position,
        Quaternion Rotation,
        Vector3 Scale)
    {
        public static TransformData FromGlobalTransform(Transform transform)
        {
            return new(transform.position, transform.rotation, transform.lossyScale);
        }
    }

    private Transform? _Transform;

    protected override TransformData CaptureValue()
    {
        return TransformData.FromGlobalTransform(_Transform ??= transform);
    }
}
