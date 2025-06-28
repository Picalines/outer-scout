using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OuterScout.Shared.DependencyInjection;
using OuterScout.Shared.Extensions;
using OuterScout.WebApi.Endpoints;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Json;
using OuterScout.WebApi.Services;
using OWML.Common;

namespace OuterScout.WebApi;

public sealed class WebApiServer : IDisposable
{
    private static readonly object[] _endpoints = new object[]
    {
        ApiEndpoint.Instance,
        CameraEndpoint.Instance,
        EnvironmentEndpoint.Instance,
        GameObjectEndpoint.Instance,
        KeyframesEndpoint.Instance,
        MeshEndpoint.Instance,
        PlayerEndpoint.Instance,
        RecorderEndpoint.Instance,
        SceneEndpoint.Instance,
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
        foreach (var endpoint in _endpoints)
        {
            if (endpoint is IRouteMapper mapper)
            {
                mapper.MapRoutes(serverBuilder);
            }
        }

        serverBuilder.MapGet("", () => $"Welcome to {nameof(OuterScout)} API!");
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
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new OuterScoutContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy
                        {
                            ProcessDictionaryKeys = false,
                        },
                    },
                    Converters =
                    {
                        Assembly
                            .GetExecutingAssembly()
                            .GetTypes()
                            .Where(type => type.IsAbstract is false)
                            .Where((typeof(JsonConverter)).IsAssignableFrom)
                            .Select(type => (JsonConverter)Activator.CreateInstance(type)),
                    },
                }
            );

        services.Register<ApiResourceRepository>().InstantiatePerUnityScene();

        services.Register<GameObjectRepository>().InstantiatePerUnityScene();

        foreach (var endpoint in _endpoints)
        {
            if (endpoint is IServiceConfiguration configuration)
            {
                configuration.RegisterServices(services);
            }
        }
    }
}
