using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;
using Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.WebApi;

[RequireComponent(typeof(OutputRecorder))]
public sealed class WebApiServer : HttpServer
{
    private sealed record RouteDefinitionContext(OutputRecorder OutputRecorder) : IApiRouteDefinition.IContext;

    private static readonly IApiRouteDefinition[] _ApiRouteDefinitions = new IApiRouteDefinition[]
    {
        TransformRouteDefinition.Instance,
        CameraInfoRouteDefinition.Instance,
        RecorderRouteDefinition.Instance,
        AnimationRouteDefinition.Instance,
        GroundBodyRouteDefinition.Instance,
    };

    public void Configure(IModConfig modConfig, IModConsole modConsole)
    {
        ModConsole = modConsole;

        var listenUrl = $"http://localhost:{modConfig.GetSettingsValue<int>("web_api_port")}/";
        var httpServerBuilder = new HttpServerBuilder(listenUrl);

        MapRoutes(httpServerBuilder);

        if (Listening is true)
        {
            StopListening();
        }

        httpServerBuilder.Build(this);
        StartListening();
    }

    private void MapRoutes(HttpServerBuilder serverBuilder)
    {
        var context = new RouteDefinitionContext(OutputRecorder: GetComponent<OutputRecorder>());

        foreach (var routeDefinition in _ApiRouteDefinitions)
        {
            routeDefinition.MapRoutes(serverBuilder, context);
        }

        serverBuilder.MapGet("", request =>
        {
            return ResponseFabric.Ok(new { Message = $"Welcome to Outer Wilds {nameof(SceneRecorder)} API!" });
        });
    }
}
