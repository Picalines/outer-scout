using SceneRecorder.Application.Extensions;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class SectorsRouteMapper : IRouteMapper
{
    public static SectorsRouteMapper Instance { get; } = new();

    private SectorsRouteMapper() { }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            serverBuilder.MapGet("player/sectors", GetPlayerSectors);
        }
    }

    private static IResponse GetPlayerSectors()
    {
        var sectorDetector = Locator.GetPlayerDetector().OrNull()?.GetComponent<SectorDetector>();

        if (sectorDetector is null)
        {
            return ServiceUnavailable();
        }

        return Ok(
            new
            {
                Current = sectorDetector.GetLastEnteredSector().transform.GetPath(),
                Sectors = sectorDetector
                    ._sectorList.Select(sector => sector.transform.GetPath())
                    .ToArray(),
            }
        );
    }
}
