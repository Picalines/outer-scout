using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.BodyMeshExport;
using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.WebApi.Extensions;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class GroundBodyRouteDefinition : IApiRouteDefinition
{
    public static GroundBodyRouteDefinition Instance { get; } = new();

    private GroundBodyRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var precondition = serverBuilder.UseInPlayableScenePrecondition();

        serverBuilder.Map(
            HttpMethod.Get,
            "ground_body/name",
            request =>
            {
                return LocatorExtensions.GetCurrentGroundBody() is { } groundBody
                    ? ResponseFabric.Ok(groundBody.name)
                    : ResponseFabric.NotFound();
            }
        );

        serverBuilder.Map(
            HttpMethod.Get,
            "ground_body/sectors/current/path",
            request =>
            {
                var playerSectorDetector = Locator
                    .GetPlayerDetector()
                    .GetComponent<SectorDetector>();
                var lastEnteredSector = playerSectorDetector.GetLastEnteredSector();

                return ResponseFabric.Ok(lastEnteredSector.transform.GetPath());
            }
        );

        serverBuilder.Map(
            HttpMethod.Post,
            "ground_body/mesh_list",
            (Request request, string output_file_path) =>
            {
                if (LocatorExtensions.GetCurrentGroundBody() is not { } groundBody)
                {
                    return ResponseFabric.NotFound();
                }

                var meshInfo = GroundBodyMeshExport.CaptureMeshInfo(groundBody.gameObject);

                File.WriteAllText(output_file_path, JsonConvert.SerializeObject(meshInfo));
                return ResponseFabric.Created();
            }
        );
    }
}
