using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.BodyMeshExport;
using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.Shared.Models;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class GroundBodyRouteDefinition : IApiRouteDefinition
{
    public static GroundBodyRouteDefinition Instance { get; } = new();

    private GroundBodyRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        serverBuilder.MapGet("ground_body/name", request =>
        {
            if (LocatorExtensions.IsInSolarSystemScene() is false)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            return LocatorExtensions.GetCurrentGroundBody() is { } groundBody
                ? ResponseFabric.Ok(groundBody.name)
                : ResponseFabric.NotFound();
        });

        serverBuilder.MapGet("ground_body/transform/global", request =>
        {
            if (LocatorExtensions.IsInSolarSystemScene() is false)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            return LocatorExtensions.GetCurrentGroundBody() is { transform: var transform }
                ? ResponseFabric.Ok(TransformModel.FromGlobalTransform(transform))
                : ResponseFabric.NotFound();
        });

        serverBuilder.MapPost("ground_body/mesh_list?{output_file_path:string}", request =>
        {
            if (LocatorExtensions.IsInSolarSystemScene() is false)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            var outputFilePath = request.GetQueryParameter<string>("output_file_path");

            if (LocatorExtensions.GetCurrentGroundBody() is not { } groundBody)
            {
                return ResponseFabric.NotFound();
            }

            var meshInfo = GroundBodyMeshExport.CaptureMeshInfo(groundBody.gameObject);

            File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(meshInfo));
            return ResponseFabric.Created();
        });
    }
}
