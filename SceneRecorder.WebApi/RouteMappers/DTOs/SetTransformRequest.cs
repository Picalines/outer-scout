using SceneRecorder.Shared.Models;

namespace SceneRecorder.WebApi.RouteMappers.DTOs;

internal sealed class SetTransformRequest
{
    public required TransformDTO Transform { get; init; }

    public required string? LocalTo { get; init; }
}