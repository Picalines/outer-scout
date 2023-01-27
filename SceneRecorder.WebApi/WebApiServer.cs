using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;
using Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.WebApi;

[RequireComponent(typeof(OutputRecorder))]
public sealed class WebApiServer : MonoBehaviour
{
    private sealed record RouteDefinitionContext(OutputRecorder OutputRecorder) : IApiRouteDefinition.IContext;

    private static readonly IApiRouteDefinition[] _ApiRouteDefinitions = new IApiRouteDefinition[]
    {
        SceneRouteDefinition.Instance,
        PlayerRouteDefinition.Instance,
        FreeCameraRouteDefinition.Instance,
        RecorderRouteDefinition.Instance,
    };

    private HttpServer? _HttpServer = null;

    public void Configure(IModConfig modConfig)
    {
        OnDestroy();

        var listenUrl = $"http://localhost:{modConfig.GetSettingsValue<int>("web_api_port")}/";
        var httpServerBuilder = new HttpServerBuilder(listenUrl);

        MapRoutes(httpServerBuilder);

        if (_HttpServer?.Listening is true)
        {
            _HttpServer.StopListening();
        }

        _HttpServer = httpServerBuilder.Build(gameObject);
        _HttpServer.StartListening();
    }

    private void OnDestroy()
    {
        _HttpServer?.StopListening();
        _HttpServer = null!;
    }

    private void MapRoutes(HttpServerBuilder serverBuilder)
    {
        var context = new RouteDefinitionContext(OutputRecorder: GetComponent<OutputRecorder>());

        foreach (var routeDefinition in _ApiRouteDefinitions)
        {
            routeDefinition.MapRoutes(serverBuilder, context);
        }
    }
}
