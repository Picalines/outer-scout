namespace SceneRecorder.WebApi.RouteMappers.DTOs;

internal sealed class CurrentSectorsResponse
{
    public required string Current { get; init; }

    public required string[] Sectors { get; init; }
}
