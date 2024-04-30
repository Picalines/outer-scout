using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;

namespace OuterScout.WebApi.Endpoints;

using static ResponseFabric;

internal sealed class EnvironmentEndpoint : IRouteMapper
{
    public static EnvironmentEndpoint Instance { get; } = new();

    private EnvironmentEndpoint() { }

    void IRouteMapper.MapRoutes(HttpServer.Builder serverBuilder)
    {
        serverBuilder.MapGet("environment", GetEnvironment);
    }

    private static IResponse GetEnvironment()
    {
        return Ok(new { OuterWildsScene = LoadManager.SceneToName(LoadManager.GetCurrentScene()) });
    }
}
