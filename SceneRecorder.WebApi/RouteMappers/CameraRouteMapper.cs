using System.Text.RegularExpressions;
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

    private static Regex _validCameraIdRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_\-]*$");

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        {
            using (serverBuilder.WithSceneCreatedFilter())
            using (serverBuilder.WithNotRecordingFilter())
            {
                serverBuilder.MapPost("cameras", CreateSceneCamera);
            }

            serverBuilder.MapGet("active-camera", GetActiveGamera);

            serverBuilder.MapGet(
                "gameObjects/:name/camera/perspective",
                GetGameObjectCameraPerspective
            );

            serverBuilder.MapPut(
                "gameObjects/:name/camera/perspective",
                PutGameObjectCameraPerspective
            );
        }
    }

    private static IResponse CreateSceneCamera([FromBody] ISceneCameraDTO cameraDTO)
    {
        var cameraId = cameraDTO.Id;

        if (_validCameraIdRegex.IsMatch(cameraId) is false)
        {
            return BadRequest("invalid camera id");
        }

        if (SceneResource.Find<ISceneCamera>(cameraId) is { })
        {
            return BadRequest($"camera with id '{cameraId}' already exists");
        }

        var parentTransform = cameraDTO.Transform.Parent is { } parentName
            ? GameObject.Find(parentName).OrNull()?.transform
            : null;

        if (cameraDTO.Transform.Parent is { } invalidParentName && parentTransform is null)
        {
            return BadRequest($"gameObject '{invalidParentName}' not found");
        }

        ISceneCamera? newCamera = cameraDTO switch
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
                        Perspective = perspectiveDTO.ToPerspective(),
                    }
                ),

            EquirectSceneCameraDTO { FaceResolution: var faceResolution }
                => EquirectSceneCamera.Create(new() { CubemapFaceSize = faceResolution }),

            _ => null,
        };

        if (newCamera is not { Transform.gameObject: var gameObject })
        {
            return BadRequest($"unknown camera type '{cameraDTO.Type}'");
        }

        newCamera.Transform.ApplyWithParent(cameraDTO.Transform.ToLocalTransform(parentTransform));

        gameObject.AddResource<ISceneCamera>(newCamera, uniqueId: cameraId);

        return Ok();
    }

    private static IResponse GetActiveGamera()
    {
        if (Locator.GetActiveCamera().OrNull() is not { } camera)
        {
            return ServiceUnavailable();
        }

        return Ok(new { Name = camera.name, Perspective = camera.GetPerspective() });
    }

    private static IResponse GetGameObjectCameraPerspective(string name)
    {
        if (GameObject.Find(name).OrNull()?.GetComponent<OWCamera>().OrNull() is not { } camera)
        {
            return NotFound();
        }

        return Ok(new { Perspective = camera.GetPerspective() });
    }

    private static IResponse PutGameObjectCameraPerspective(
        string name,
        [FromBody] CameraPerspectiveDTO perspectiveDTO
    )
    {
        if (GameObject.Find(name).OrNull()?.GetComponent<OWCamera>().OrNull() is not { } camera)
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
