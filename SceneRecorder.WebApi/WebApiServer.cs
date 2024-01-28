using OWML.Common;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Application.Recording;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.RouteMappers;
using UnityEngine;

namespace SceneRecorder.WebApi;

[RequireComponent(typeof(OutputRecorder))]
public sealed class WebApiServer : HttpServer
{
    private sealed record RouteDefinitionContext(OutputRecorder OutputRecorder)
        : IRouteMapper.IContext;

    private static readonly IRouteMapper[] _RouteMappers = new IRouteMapper[]
    {
        CameraInfoRouteMapper.Instance,
        GroundBodyRouteMapper.Instance,
        KeyframesRouteMapper.Instance,
        RecorderRouteMapper.Instance,
        SectorsRouteMapper.Instance,
        TransformRouteMapper.Instance,
        WarpRouteMapper.Instance,
    };

    public void Configure(IModConfig modConfig, IModConsole modConsole)
    {
        ModConsole = modConfig.GetEnableApiInfoLogsSetting()
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

        foreach (var mapper in _RouteMappers)
        {
            mapper.MapRoutes(serverBuilder, context);
        }

        serverBuilder.MapGet(
            "",
            () => new { Message = $"Welcome to Outer Wilds {nameof(SceneRecorder)} API!" }
        );

        serverBuilder.MapGet("api-status", () => new { Available = true });
    }
}
