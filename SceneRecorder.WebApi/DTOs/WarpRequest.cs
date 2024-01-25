namespace SceneRecorder.WebApi.DTOs;

internal sealed class WarpRequest
{
    public required TransformDTO LocalTransform { get; init; }
}
