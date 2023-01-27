using Picalines.OuterWilds.SceneRecorder.WebApi.Http;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class SceneRouteDefinition : IApiRouteDefinition
{
    public static SceneRouteDefinition Instance { get; } = new();

    private SceneRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        serverBuilder.MapGet("scene/settings", request =>
        {
            return context.OutputRecorder switch
            {
                { SceneSettings: { } sceneSettings } => ResponseFabric.Ok(sceneSettings),
                _ => ResponseFabric.ServiceUnavailable(),
            };
        });
    }
}
