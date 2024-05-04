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

    private static readonly string _openApiSpecResource;

    private static readonly string _swaggerResource;

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
            return NotFound(
                new Problem("unknownApiSpec") { Detail = $"API spec of type '{type}' was not found" }
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
}
