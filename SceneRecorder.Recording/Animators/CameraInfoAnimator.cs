using SceneRecorder.Shared.Models;

namespace SceneRecorder.Recording.Animators;

internal sealed class CameraInfoAnimator : Animator<CameraDTO>
{
    private readonly OWCamera _TargetCamera;

    public CameraInfoAnimator(OWCamera targetCamera)
        : base(CameraDTO.FromOWCamera(targetCamera))
    {
        _TargetCamera = targetCamera;
    }

    protected override void ApplyValue(CameraDTO value)
    {
        value.Apply(_TargetCamera);
    }
}
