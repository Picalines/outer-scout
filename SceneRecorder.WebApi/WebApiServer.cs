using OWML.Common;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Recording.Recorders;
using SceneRecorder.Shared.Extensions;
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
        CameraInfoRouteDefinition.Instance,
        GroundBodyRouteDefinition.Instance,
        KeyframesRouteDefinition.Instance,
        RecorderRouteDefinition.Instance,
        SectorsRouteDefinition.Instance,
        TransformRouteDefinition.Instance,
        WarpRouteDefinition.Instance,
    };

    public void Configure(IModConfig modConfig, IModConsole modConsole)
    {
        var enableInfoLogs = modConfig.GetEnableApiInfoLogsSetting();
        ModConsole = enableInfoLogs
            ? modConsole
            : modConsole.WithOnlyMessagesOfType(MessageType.Warning, MessageType.Error);

        var listenUrl = $"http://localhost:{modConfig.GetApiPortSetting()}/";
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
