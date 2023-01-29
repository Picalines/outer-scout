using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.Shared.Models;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class PlayerRouteDefinition : IApiRouteDefinition
{
    public static PlayerRouteDefinition Instance { get; } = new();

    private PlayerRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        serverBuilder.MapGet("player/transform/global", request =>
        {
            return LocatorExtensions.IsInSolarSystemScene()
                ? ResponseFabric.Ok(TransformModel.FromGlobalTransform(Locator.GetPlayerBody().transform))
                : ResponseFabric.ServiceUnavailable();
        });
    }
}
