using System.Reflection;
using Newtonsoft.Json;
using OWML.Common;
using SceneRecorder.Infrastructure.DependencyInjection;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.RouteMappers;

namespace SceneRecorder.WebApi;

public sealed class WebApiServer : IDisposable
{
    private static readonly IRouteMapper[] _RouteMappers = new IRouteMapper[]
    {
        CameraRouteMapper.Instance,
        GroundBodyRouteMapper.Instance,
        KeyframesRouteMapper.Instance,
        RecorderRouteMapper.Instance,
        SectorsRouteMapper.Instance,
        TransformRouteMapper.Instance,
        WarpRouteMapper.Instance,
    };

    private readonly HttpServer _httpServer;

    public WebApiServer()
    {
        var modConfig = Singleton<IModConfig>.Instance;

        var httpServerBuilder = new HttpServer.Builder(
            $"http://localhost:{modConfig.GetApiPortSetting()}/"
        );

        ConfigureServices(httpServerBuilder);

        MapRoutes(httpServerBuilder);

        _httpServer = httpServerBuilder.Build();
    }

    public void Dispose()
    {
        _httpServer.Dispose();
    }

    private void ConfigureServices(HttpServer.Builder serverBuilder)
    {
        var modConfig = Singleton<IModConfig>.Instance;
        var modConsole = Singleton<IModConsole>.Instance;

        serverBuilder.Services.RegisterInstance<IModConsole>(
            modConfig.GetEnableApiInfoLogsSetting()
                ? modConsole
                : modConsole.WithOnlyMessagesOfType(MessageType.Warning, MessageType.Error)
        );

        serverBuilder.Services.RegisterInstance(
            new JsonSerializer()
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                Converters =
                {
                    Assembly
                        .GetExecutingAssembly()
                        .GetTypes()
                        .Where(type => type.IsAbstract is false)
                        .Where(type => type.IsSubclassOf(typeof(JsonConverter)))
                        .Select(type => (JsonConverter)Activator.CreateInstance(type))
                },
            }
        );
    }

    private void MapRoutes(HttpServer.Builder serverBuilder)
    {
        foreach (var mapper in _RouteMappers)
        {
            mapper.MapRoutes(serverBuilder);
        }

        serverBuilder.MapGet("", () => $"Welcome to Outer Wilds {nameof(SceneRecorder)} API!");

        serverBuilder.MapGet("api-status", () => new { Available = true });
    }
}
