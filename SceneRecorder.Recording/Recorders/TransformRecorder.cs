using Picalines.OuterWilds.SceneRecorder.Recording.Recorders.Abstract;
using Picalines.OuterWilds.SceneRecorder.Shared.Models;
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
