using System.Text.RegularExpressions;
using OuterScout.Application.Extensions;
using OuterScout.Application.SceneCameras;
using OuterScout.Domain;
using OuterScout.Infrastructure.Extensions;
using OuterScout.WebApi.DTOs;
using OuterScout.WebApi.Extensions;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Services;
using UnityEngine;

namespace OuterScout.WebApi.Endpoints;

using static ResponseFabric;

internal sealed class CameraEndpoint : IRouteMapper
{
    public static CameraEndpoint Instance { get; } = new();

    private CameraEndpoint() { }

    private static Regex _validCameraIdRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_\-]*$");

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        {
            using (serverBuilder.WithSceneCreatedFilter())
            using (serverBuilder.WithNotRecordingFilter())
            {
                serverBuilder.MapPost("cameras", CreateSceneCamera);

                serverBuilder.MapPut("cameras/:id/perspective", PutGameObjectCameraPerspective);

                serverBuilder.MapPut(
                    "gameObjects/:name/camera/perspective",
                    PutGameObjectCameraPerspective
                );
            }

            serverBuilder.MapGet("scene/active-camera", GetActiveGamera);

            serverBuilder.MapGet(
                "gameObjects/:name/camera/perspective",
                GetGameObjectCameraPerspective
            );
        }
    }

    private static IResponse CreateSceneCamera(
        [FromBody] ISceneCameraDTO cameraDTO,
        GameObjectRepository gameObjects,
        ApiResourceRepository resources
    )
    {
        var cameraId = cameraDTO.Id;

        if (_validCameraIdRegex.IsMatch(cameraId) is false)
        {
            return BadRequest("invalid camera id");
        }

        if (resources.GlobalContainer.GetResource<ISceneCamera>(cameraId) is { })
        {
            return BadRequest($"camera with id '{cameraId}' already exists");
        }

        var parentTransform = cameraDTO.Transform.Parent is { } parentName
            ? gameObjects.FindOrNull(parentName)?.transform
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
                Perspective: var perspective
            }
                => PerspectiveSceneCamera.Create(
                    new()
                    {
                        Resolution = new Vector2Int(resolution.Width, resolution.Height),
                        GateFit = gateFit,
                        Perspective = perspective,
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

        resources.GlobalContainer.AddResource<ISceneCamera>(cameraId, newCamera);

        return Created();
    }

    private static IResponse GetActiveGamera()
    {
        if (Locator.GetActiveCamera().OrNull() is not { } camera)
        {
            return ServiceUnavailable();
        }

        return Ok(new { Name = camera.name });
    }

    private static IResponse PutCameraPerspective(
        [FromUrl] string id,
        [FromBody] CameraPerspective perspective,
        ApiResourceRepository resources
    )
    {
        if (resources.GlobalContainer.GetResource<ISceneCamera>(id) is not { } camera)
        {
            return NotFound($"camera '{id}' not found");
        }

        if (camera is not PerspectiveSceneCamera perspectiveCamera)
        {
            return BadRequest($"camera '{id}' is not perspective");
        }

        perspectiveCamera.Perspective = perspective;

        return Ok();
    }

    private static IResponse GetGameObjectCameraPerspective(
        [FromUrl] string name,
        GameObjectRepository gameObjects
    )
    {
        if (gameObjects.FindOrNull(name)?.GetComponentOrNull<OWCamera>() is not { } camera)
        {
            return NotFound();
        }

        if (camera.mainCamera.usePhysicalProperties is false)
        {
            return BadRequest($"camera '{name}' does not use physical properties");
        }

        return Ok(new { Perspective = camera.GetPerspective() });
    }

    private static IResponse PutGameObjectCameraPerspective(
        [FromUrl] string name,
        [FromBody] CameraPerspective perspective,
        GameObjectRepository gameObjects
    )
    {
        if (gameObjects.FindOrNull(name)?.GetComponentOrNull<OWCamera>() is not { } camera)
        {
            return NotFound();
        }

        if (Locator.GetPlayerCamera() == camera)
        {
            return ServiceUnavailable($"can't modify player camera");
        }

        camera.mainCamera.usePhysicalProperties = true;

        camera.ApplyPerspective(perspective);

        return Ok();
    }
}
