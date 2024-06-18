using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using OuterScout.Infrastructure.DependencyInjection;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OWML.Common;

namespace OuterScout.WebApi.Endpoints;

using static ResponseFabric;

internal sealed class ApiEndpoint : IRouteMapper, IServiceConfiguration
{
    private record struct ApiVersion(int Major, int Minor, int Patch);

    public static ApiEndpoint Instance { get; } = new();

    private static readonly Assembly _assembly;

    private static readonly string _openApiSpecResource;

    private static readonly string _swaggerResource;

    private static ApiVersion _apiVersion;

    private ApiEndpoint() { }

    static ApiEndpoint()
    {
        _assembly = Assembly.GetAssembly(typeof(ApiEndpoint));
        var resourceNames = _assembly.GetManifestResourceNames();

        _openApiSpecResource = resourceNames.First(name => name.Contains("openapi.yaml"));
        _swaggerResource = resourceNames.First(name => name.Contains("swagger.html"));
    }

    void IRouteMapper.MapRoutes(HttpServer.Builder serverBuilder)
    {
        serverBuilder.MapGet("api/version", GetApiVersion);

        serverBuilder.MapGet("api/spec", GetApiSpecification);

        serverBuilder.MapGet("api/swagger", GetSwaggerUI);
    }

    void IServiceConfiguration.RegisterServices(ServiceContainer.Builder services)
    {
        var manifest = Singleton<IModManifest>.Instance;

        _apiVersion =
            ParseVersionString(manifest.Version)
            ?? throw new InvalidOperationException(
                $"{nameof(OuterScout)} version in the manifest is not correct. Expected format is X.Y.Z"
            );
    }

    private static IResponse GetApiVersion()
    {
        return Ok(_apiVersion);
    }

    private static IResponse GetApiSpecification([FromUrl] string type)
    {
        if (type is not "openapi")
        {
            return NotFound(
                new Problem("unknownApiSpec")
                {
                    Detail = $"API spec of type '{type}' was not found"
                }
            );
        }

        var stream = _assembly.GetManifestResourceStream(_openApiSpecResource);

        return new StreamResponse(HttpStatusCode.OK, stream) { ContentType = "application/yaml" };
    }

    private static IResponse GetSwaggerUI()
    {
        var stream = _assembly.GetManifestResourceStream(_swaggerResource);

        return new StreamResponse(HttpStatusCode.OK, stream) { ContentType = "text/html" };
    }

    private static ApiVersion? ParseVersionString(string version)
    {
        var versionRegex = new Regex(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)$");
        var versionMatch = versionRegex.Match(version);

        if (versionMatch is null)
        {
            return null;
        }

        var majorVersion = int.Parse(versionMatch.Groups["major"].Value);
        var minorVersion = int.Parse(versionMatch.Groups["minor"].Value);
        var patchVersion = int.Parse(versionMatch.Groups["patch"].Value);

        return new()
        {
            Major = majorVersion,
            Minor = minorVersion,
            Patch = patchVersion
        };
    }
}
