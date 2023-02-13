using Picalines.OuterWilds.SceneRecorder.Shared.Models;

namespace Picalines.OuterWilds.SceneRecorder.Recording.Animators;

internal sealed class CameraInfoAnimator : Animator<CameraInfo>
{
    private readonly OWCamera _TargetCamera;

    public CameraInfoAnimator(OWCamera targetCamera)
        : base(CameraInfo.FromOWCamera(targetCamera))
    {
        _TargetCamera = targetCamera;
    }

    protected override void ApplyValue(CameraInfo value)
    {
        value.ApplyToOWCamera(_TargetCamera);
    }
}
