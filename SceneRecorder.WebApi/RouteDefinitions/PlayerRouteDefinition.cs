using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.BodyMeshExport;
using Picalines.OuterWilds.SceneRecorder.Models;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class PlayerRouteDefinition : IApiRouteDefinition
{
    public static PlayerRouteDefinition Instance { get; } = new();

    private PlayerRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        serverBuilder.MapGet("player/transform", request =>
        {
            return LocatorExtensions.IsInSolarSystemScene()
                ? ResponseFabric.Ok(TransformModel.FromGlobalTransform(Locator.GetPlayerBody().transform))
                : ResponseFabric.ServiceUnavailable();
        });

        serverBuilder.MapGet("player/ground_body", request =>
        {
            if (LocatorExtensions.IsInSolarSystemScene() is false)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            var groundBody = Locator.GetPlayerController().GetLastGroundBodySafe();

            return ResponseFabric.Ok(new
            {
                groundBody.name,
                transform = TransformModel.FromGlobalTransform(groundBody.transform),
            });
        });

        serverBuilder.MapPost("player/ground_body/mesh_list?{output_file_path:string}", request =>
        {
            if (LocatorExtensions.IsInSolarSystemScene() is false)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            var outputFilePath = request.GetQueryParameter<string>("output_file_path");

            var groundBody = Locator.GetPlayerController().GetLastGroundBodySafe();
            var meshInfo = GroundBodyMeshExport.CaptureMeshInfo(groundBody.gameObject);

            File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(meshInfo));
            return ResponseFabric.Created();
        });
    }
}
