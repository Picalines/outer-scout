using SceneRecorder.Application.Extensions;
using SceneRecorder.Application.SceneCameras;
using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.DTOs;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class CameraRouteMapper : IRouteMapper
{
    public static CameraRouteMapper Instance { get; } = new();

    private CameraRouteMapper() { }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            serverBuilder.MapPost("cameras", CreateSceneCamera);

            serverBuilder.MapGet("active-camera/perspective", GetActiveCameraPerspective);

            serverBuilder.MapPut("active-camera/perspective", PutActiveCameraPerspective);
        }
    }

    private static IResponse CreateSceneCamera([FromBody] ISceneCameraDTO cameraDTO)
    {
        var cameraId = cameraDTO.Id;

        if (SceneResource.Find<ISceneCamera>(cameraId) is { })
        {
            return BadRequest($"camera with id '{cameraId}' already exists");
        }

        var newCamera = cameraDTO switch
        {
            PerspectiveSceneCameraDTO
            {
                Resolution: var resolution,
                GateFit: var gateFit,
                Perspective: var perspectiveDTO
            }
                => PerspectiveSceneCamera.Create(
                    new()
                    {
                        Resolution = new Vector2Int(resolution.Width, resolution.Height),
                        GateFit = gateFit,
                        Perspective = perspectiveDTO.ToPerspective()
                    }
                ),
            _ => null,
        };

        if (newCamera is null)
        {
            return BadRequest($"unknown camera type '{cameraDTO.Type}'");
        }

        newCamera.gameObject.AddResource<ISceneCamera>(newCamera, uniqueId: cameraId);

        return Ok();
    }

    public static IResponse GetActiveCameraPerspective()
    {
        if (Locator.GetActiveCamera().OrNull() is not { } camera)
        {
            return ServiceUnavailable();
        }

        return Ok(new { Perspective = camera.GetPerspective() });
    }

    public static IResponse PutActiveCameraPerspective(
        [FromBody] CameraPerspectiveDTO perspectiveDTO
    )
    {
        if (Locator.GetActiveCamera().OrNull() is not { } camera)
        {
            return NotFound();
        }

        if (Locator.GetPlayerCamera() == camera)
        {
            return ServiceUnavailable($"can't modify player camera");
        }

        camera.ApplyPerspective(perspectiveDTO.ToPerspective());

        return Ok();
    }
}
