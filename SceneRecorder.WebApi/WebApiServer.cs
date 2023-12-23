using OWML.Common;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Recording.Recorders;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.RouteDefinitions;
using UnityEngine;

namespace SceneRecorder.WebApi;

[RequireComponent(typeof(OutputRecorder))]
public sealed class WebApiServer : HttpServer
{
    private sealed record RouteDefinitionContext(OutputRecorder OutputRecorder)
        : IApiRouteDefinition.IContext;

    private static readonly IApiRouteDefinition[] _ApiRouteDefinitions = new IApiRouteDefinition[]
    {
        TransformRouteDefinition.Instance,
        CameraInfoRouteDefinition.Instance,
        RecorderRouteDefinition.Instance,
        AnimationRouteDefinition.Instance,
        GroundBodyRouteDefinition.Instance,
        WarpRouteDefinition.Instance,
    };

    public void Configure(IModConfig modConfig, IModConsole modConsole)
    {
        var logOnlyErrors = modConfig.GetSettingsValue<bool>("Web API log only errors");
        ModConsole = logOnlyErrors
            ? modConsole.WithOnlyMessagesOfType(MessageType.Error)
            : modConsole;

        var listenUrl = $"http://localhost:{modConfig.GetSettingsValue<int>("Web API port")}/";
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
        var context = new RouteDefinitionContext(GetComponent<OutputRecorder>());

        foreach (var routeDefinition in _ApiRouteDefinitions)
        {
            routeDefinition.MapRoutes(serverBuilder, context);
        }

        serverBuilder.MapGet(
            "",
            () => new { message = $"Welcome to Outer Wilds {nameof(SceneRecorder)} API!" }
        );
    }
}
