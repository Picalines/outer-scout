using SceneRecorder.Shared.Extensions;
using SceneRecorder.WebApi.DTOs;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class SectorsRouteMapper : IRouteMapper
{
    public static SectorsRouteMapper Instance { get; } = new();

    private SectorsRouteMapper() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IRouteMapper.IContext context)
    {
        using var precondition = serverBuilder.UseInPlayableScenePrecondition();

        serverBuilder.MapGet(
            "player/sectors",
            () =>
            {
                var sectorDetector = Locator
                    .GetPlayerDetector()
                    .OrNull()
                    ?.GetComponent<SectorDetector>();

                if (sectorDetector is null)
                {
                    return ServiceUnavailable();
                }

                return Ok(
                    new CurrentSectorsResponse
                    {
                        Current = sectorDetector.GetLastEnteredSector().transform.GetPath(),
                        Sectors = sectorDetector
                            ._sectorList.Select(sector => sector.transform.GetPath())
                            .ToArray(),
                    }
                );
            }
        );
    }
}
