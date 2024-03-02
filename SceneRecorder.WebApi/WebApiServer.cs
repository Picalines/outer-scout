using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OWML.Common;
using SceneRecorder.Application.Extensions;
using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.DependencyInjection;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Infrastructure.Validation;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.RouteMappers;
using SceneRecorder.WebApi.Services;

namespace SceneRecorder.WebApi;

using SceneRecorder.Application.Recording;

public sealed class WebApiServer : IDisposable
{
    private static readonly IRouteMapper[] _RouteMappers = new IRouteMapper[]
    {
        CameraRouteMapper.Instance,
        GroundBodyRouteMapper.Instance,
        KeyframesRouteMapper.Instance,
        PlayerRouteMapper.Instance,
        RecorderRouteMapper.Instance,
        SceneRouteMapper.Instance,
        TransformRouteMapper.Instance,
    };

    private readonly HttpServer _httpServer;

    public WebApiServer()
    {
        var modConfig = Singleton<IModConfig>.Instance;

        var services = new ServiceContainer.Builder();

        var httpServerBuilder = new HttpServer.Builder(
            $"http://localhost:{modConfig.GetApiPortSetting()}/",
            services
        );

        RegisterServices(services);

        MapRoutes(httpServerBuilder);

        _httpServer = httpServerBuilder.Build();
    }

    public void Dispose()
    {
        _httpServer.Dispose();
    }

    private void MapRoutes(HttpServer.Builder serverBuilder)
    {
        foreach (var mapper in _RouteMappers)
        {
            mapper.MapRoutes(serverBuilder);
        }

        serverBuilder.MapGet("", () => $"Welcome to Outer Wilds {nameof(SceneRecorder)} API!");

        serverBuilder.MapGet("api/status", () => new { Available = true });
    }

    private static void RegisterServices(ServiceContainer.Builder services)
    {
        services
            .Register<IModConfig>()
            .AsSingleton()
            .InstantiateBy(() => Singleton<IModConfig>.Instance);

        services
            .Register<IModConsole>()
            .AsSingleton()
            .InstantiateBy(services =>
            {
                var modConsole = Singleton<IModConsole>.Instance;
                return services.Resolve<IModConfig>().GetEnableApiInfoLogsSetting()
                    ? modConsole
                    : modConsole.WithOnlyMessagesOfType(MessageType.Warning, MessageType.Error);
            });

        services
            .Override<JsonSerializer>()
            .AsExternalReference(
                new JsonSerializer()
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy
                        {
                            ProcessDictionaryKeys = false
                        }
                    },
                    Converters =
                    {
                        Assembly
                            .GetExecutingAssembly()
                            .GetTypes()
                            .Where(type => type.IsAbstract is false)
                            .Where((typeof(JsonConverter)).IsAssignableFrom)
                            .Select(type => (JsonConverter)Activator.CreateInstance(type))
                    },
                }
            );

        services.Register<GameObjectRepository>().InstantiatePerUnityScene();

        services
            .Register<ResettableLazy<SceneRecorder.Builder>>()
            .InstantiatePerUnityScene()
            .InstantiateBy(
                () =>
                    ResettableLazy.Of(() =>
                    {
                        LocatorExtensions.IsInPlayableScene().Throw().IfFalse();
                        return new SceneRecorder.Builder();
                    })
            );

        services
            .Register<SceneRecorder.Builder>()
            .InstantiatePerResolve()
            .InstantiateBy(container =>
            {
                return container.Resolve<ResettableLazy<SceneRecorder.Builder>>().Value;
            });
    }
}
