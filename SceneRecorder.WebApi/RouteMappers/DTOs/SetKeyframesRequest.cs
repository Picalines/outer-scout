namespace SceneRecorder.WebApi.RouteMappers.DTOs;

internal sealed class SetKeyframesRequest<T>
{
    public required T[] Values { get; init; }

    public required int FromFrame { get; init; }
}
