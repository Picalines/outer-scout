using SceneRecorder.Recording.SceneCameras;

namespace SceneRecorder.Recording.Animators;

public sealed class CameraInfoApplier : IValueApplier<CameraInfo>
{
    public required SceneCamera TargetCamera { get; init; }

    public void Apply(CameraInfo value)
    {
        TargetCamera.CameraInfo = value;
    }
}
