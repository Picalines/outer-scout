using System.Net;
using System.Reflection;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;

namespace OuterScout.WebApi.Endpoints;

using static ResponseFabric;

internal sealed class ApiEndpoint : IRouteMapper
{
    public static ApiEndpoint Instance { get; } = new();

    private static readonly Assembly _assembly;

    private static readonly string _openApiSpenResource;

    private ApiEndpoint() { }

    static ApiEndpoint()
    {
        _assembly = Assembly.GetAssembly(typeof(ApiEndpoint));
        _openApiSpenResource = _assembly
            .GetManifestResourceNames()
            .First(name => name.Contains("openapi.yaml"));
    }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        serverBuilder.MapGet("api/version", GetApiVersion);

        serverBuilder.MapGet("api/spec", GetApiSpecification);
    }

    private static IResponse GetApiVersion()
    {
        return Ok(
            new
            {
                Major = 0,
                Minor = 1,
                Patch = 0
            }
        );
    }

    private static IResponse GetApiSpecification([FromUrl] string type)
    {
        if (type is not "openapi")
        {
            return NotFound($"API spec of type '{type}' was not found");
        }

        var stream = _assembly.GetManifestResourceStream(_openApiSpenResource);

        return new StreamResponse(HttpStatusCode.OK, stream) { ContentType = "application/yaml" };
    }
}
