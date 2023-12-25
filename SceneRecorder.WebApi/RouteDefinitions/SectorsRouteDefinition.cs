using SceneRecorder.Shared.Extensions;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;

namespace SceneRecorder.WebApi.RouteDefinitions;

internal sealed class SectorsRouteDefinition : IApiRouteDefinition
{
    public static SectorsRouteDefinition Instance { get; } = new();

    private SectorsRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var precondition = serverBuilder.UseInPlayableScenePrecondition();

        serverBuilder.MapGet(
            "player/sectors",
            () =>
            {
                var sectorDetector = Locator.GetPlayerDetector().GetComponent<SectorDetector>();

                return new
                {
                    Current = sectorDetector.GetLastEnteredSector().transform.GetPath(),
                    Sectors = sectorDetector
                        ._sectorList.Select(sector => sector.transform.GetPath())
                        .ToArray(),
                };
            }
        );
    }
}
