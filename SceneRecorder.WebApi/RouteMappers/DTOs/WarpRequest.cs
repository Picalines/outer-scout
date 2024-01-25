using SceneRecorder.Shared.DTOs;

namespace SceneRecorder.WebApi.RouteMappers.DTOs;

internal sealed class WarpRequest
{
    public required TransformDTO LocalTransform { get; init; }
}
