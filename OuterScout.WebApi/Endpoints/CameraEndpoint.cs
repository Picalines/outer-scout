using System.Text.RegularExpressions;
using JsonSubTypes;
using Newtonsoft.Json;
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
            using (serverBuilder.WithNotRecordingFilter())
            {
                using (serverBuilder.WithSceneCreatedFilter())
                {
                    serverBuilder.MapPost("cameras", CreateSceneCamera);

                    serverBuilder.MapPut("cameras/:id/perspective", PutCameraPerspective);

                    serverBuilder.MapPut("cameras/:id/transform", PutCameraTransform);
                }

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

    [JsonConverter(typeof(JsonSubtypes), nameof(ISceneCameraDto.Type))]
    [JsonSubtypes.KnownSubType(typeof(PerspectiveSceneCameraDto), "perspective")]
    [JsonSubtypes.KnownSubType(typeof(EquirectSceneCameraDto), "equirectangular")]
    private interface ISceneCameraDto
    {
        public string Id { get; }

        public string Type { get; }

        public TransformDto Transform { get; }
    }

    private sealed class ResolutionDto
    {
        public required int Width { get; init; }

        public required int Height { get; init; }
    }

    private sealed class PerspectiveSceneCameraDto : ISceneCameraDto
    {
        public required string Id { get; init; }

        public required string Type { get; init; } = "perspective";

        public required TransformDto Transform { get; init; }

        public required Camera.GateFitMode GateFit { get; init; }

        public required ResolutionDto Resolution { get; init; }

        public required CameraPerspective Perspective { get; init; }
    }

    private sealed class EquirectSceneCameraDto : ISceneCameraDto
    {
        public required string Id { get; init; }

        public required string Type { get; init; } = "equirectangular";

        public required TransformDto Transform { get; init; }

        public required int FaceResolution { get; init; }
    }

    private static IResponse CreateSceneCamera(
        [FromBody] ISceneCameraDto cameraDto,
        GameObjectRepository gameObjects,
        ApiResourceRepository resources
    )
    {
        var cameraId = cameraDto.Id;

        if (_validCameraIdRegex.IsMatch(cameraId) is false)
        {
            return BadRequest("invalid camera id");
        }

        if (resources.GlobalContainer.GetResource<ISceneCamera>(cameraId) is { })
        {
            return BadRequest($"camera with id '{cameraId}' already exists");
        }

        var parent = cameraDto.Transform.Parent is { } parentName
            ? gameObjects.FindOrNull(parentName)?.transform
            : null;

        if ((cameraDto.Transform.Parent, parent) is (not null, null))
        {
            return CommonResponse.GameObjectNotFound(cameraDto.Transform.Parent);
        }

        parent ??= resources
            .GlobalContainer.GetRequiredResource<GameObject>(SceneEndpoint.OriginResource)
            .transform;

        ISceneCamera? newCamera = cameraDto switch
        {
            PerspectiveSceneCameraDto
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

            EquirectSceneCameraDto { FaceResolution: var faceResolution }
                => EquirectSceneCamera.Create(new() { CubemapFaceSize = faceResolution }),

            _ => null,
        };

        if (newCamera is null)
        {
            return BadRequest($"unknown camera type '{cameraDto.Type}'");
        }

        newCamera.Transform.parent = parent;
        newCamera.Transform.ResetLocal();
        cameraDto.Transform.ApplyLocal(newCamera.Transform);

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
            return CommonResponse.CameraNotFound(id);
        }

        if (camera is not PerspectiveSceneCamera perspectiveCamera)
        {
            return BadRequest($"camera '{id}' is not perspective");
        }

        perspectiveCamera.Perspective = perspective;

        return Ok();
    }

    private static IResponse PutCameraTransform(
        [FromUrl] string id,
        [FromBody] TransformDto transformDto,
        ApiResourceRepository resources,
        GameObjectRepository gameObjects
    )
    {
        if (
            resources.GlobalContainer.GetResource<ISceneCamera>(id)
            is not { Transform: var transform }
        )
        {
            return CommonResponse.CameraNotFound(id);
        }

        var parent = transformDto.Parent is { } parentName
            ? gameObjects.FindOrNull(parentName)?.transform
            : null;

        if ((transformDto.Parent, parent) is (not null, null))
        {
            return CommonResponse.GameObjectNotFound(transformDto.Parent);
        }

        parent ??= resources
            .GlobalContainer.GetRequiredResource<GameObject>(SceneEndpoint.OriginResource)
            .transform;

        transformDto.ApplyGlobal(transform, parent);

        return Ok();
    }

    private static IResponse GetGameObjectCameraPerspective(
        [FromUrl] string name,
        GameObjectRepository gameObjects
    )
    {
        if (gameObjects.FindOrNull(name)?.GetComponentOrNull<OWCamera>() is not { } camera)
        {
            return CommonResponse.GameObjectNotFound(name);
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
            return CommonResponse.CameraNotFound(name);
        }

        if (Locator.GetPlayerCamera() == camera)
        {
            return MethodNotAllowed("can't modify player camera");
        }

        camera.mainCamera.usePhysicalProperties = true;

        camera.ApplyPerspective(perspective);

        return Ok();
    }
}
