using Picalines.OuterWilds.SceneRecorder.Models;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording;

public sealed class TransformRecorder : ValueRecorder<TransformModel>
{
    private Transform? _Transform;

    protected override TransformModel CaptureValue()
    {
        return TransformModel.FromGlobalTransform(_Transform ??= transform);
    }
}
