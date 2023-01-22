using Picalines.OuterWilds.SceneRecorder.Models;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders.Abstract;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording.Recorders;

internal sealed class TransformRecorder : ValueRecorder<TransformModel>
{
    private Transform? _Transform;

    protected override TransformModel CaptureValue()
    {
        return TransformModel.FromGlobalTransform(_Transform ??= transform);
    }
}
