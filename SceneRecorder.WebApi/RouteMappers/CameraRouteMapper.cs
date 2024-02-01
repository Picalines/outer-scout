using SceneRecorder.WebApi.DTOs;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class CameraRouteMapper : IRouteMapper
{
    public static CameraRouteMapper Instance { get; } = new();

    private CameraRouteMapper() { }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.UseInPlayableSceneFilter())
        {
            serverBuilder.MapPost("cameras", CreateSceneCamera);

            serverBuilder.MapGet("cameras/:id/perspective-info", GetCameraPerspectiveInfo);

            serverBuilder.MapPut("cameras/:id/perspective-info", PutCameraPerspectiveInfo);
        }
    }

    private static IResponse CreateSceneCamera([FromBody] ISceneCameraDTO cameraDTO)
    {
        throw new NotImplementedException();
    }

    public static IResponse GetCameraPerspectiveInfo(string id)
    {
        throw new NotImplementedException();
    }

    public static IResponse PutCameraPerspectiveInfo(string id)
    {
        throw new NotImplementedException();
    }
}
