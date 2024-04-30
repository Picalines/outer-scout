using JsonSubTypes;
using Newtonsoft.Json;
using OuterScout.Application.Extensions;
using OuterScout.Application.SceneCameras;
using OuterScout.Domain;
using OuterScout.Infrastructure.Extensions;
using OuterScout.Infrastructure.Validation;
using OuterScout.WebApi.Extensions;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Services;
using UnityEngine;

namespace OuterScout.WebApi.Endpoints;

using static ResponseFabric;

internal sealed class CameraEndpoint : IRouteMapper
{
    public static class CameraType
    {
        public const string Perspective = "perspective";

        public const string Equirect = "equirectangular";

        public const string Unity = "unity";
    }

    public static CameraEndpoint Instance { get; } = new();

    private CameraEndpoint() { }

    void IRouteMapper.MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            using (serverBuilder.WithSceneCreatedFilter())
            {
                serverBuilder.MapPost("objects/:name/camera", PostCamera);
            }

            serverBuilder.MapGet("objects/:name/camera", GetCamera);

            serverBuilder.MapGet("scene/active-camera", GetActiveGamera);

            serverBuilder.MapPut("objects/:name/camera", PutCamera);
        }
    }

    [JsonConverter(typeof(JsonSubtypes), nameof(IPostCameraRequest.Type))]
    [JsonSubtypes.KnownSubType(typeof(PostPerspectiveSceneCameraRequest), CameraType.Perspective)]
    [JsonSubtypes.KnownSubType(typeof(PostEquirectSceneCameraRequest), CameraType.Equirect)]
    private interface IPostCameraRequest
    {
        public string Type { get; }
    }

    private sealed class ResolutionDto
    {
        public required int Width { get; init; }

        public required int Height { get; init; }
    }

    private sealed class PostPerspectiveSceneCameraRequest : IPostCameraRequest
    {
        public required string Type { get; init; } = CameraType.Perspective;

        public required Camera.GateFitMode GateFit { get; init; }

        public required ResolutionDto Resolution { get; init; }

        public required CameraPerspective Perspective { get; init; }
    }

    private sealed class PostEquirectSceneCameraRequest : IPostCameraRequest
    {
        public required string Type { get; init; } = CameraType.Equirect;

        public required int FaceResolution { get; init; }
    }

    private static IResponse PostCamera(
        [FromUrl] string name,
        [FromBody] IPostCameraRequest request,
        GameObjectRepository gameObjects,
        ApiResourceRepository resources
    )
    {
        if (gameObjects.GetOwnOrNull(name) is not { } gameObject)
        {
            return CommonResponse.GameObjectIsNotCustom(name);
        }

        var container = resources.ContainerOf(gameObject);

        if (container.GetResource<ISceneCamera>() is not null)
        {
            return BadRequest(
                new { Error = $"gameObject '{name}' already contains camera component" }
            );
        }

        ISceneCamera? newCamera = request switch
        {
            PostPerspectiveSceneCameraRequest
            {
                Resolution: var resolution,
                GateFit: var gateFit,
                Perspective: var perspective
            }
                => PerspectiveSceneCamera.Create(
                    gameObject,
                    new()
                    {
                        Resolution = new Vector2Int(resolution.Width, resolution.Height),
                        GateFit = gateFit,
                        Perspective = perspective,
                    }
                ),

            PostEquirectSceneCameraRequest { FaceResolution: var faceResolution }
                => EquirectSceneCamera.Create(
                    gameObject,
                    new() { CubemapFaceSize = faceResolution }
                ),

            _ => null,
        };

        newCamera.AssertNotNull();

        container.AddResource<ISceneCamera>(name, newCamera);

        return Created();
    }

    private sealed class CameraResponse
    {
        public required string Type { get; init; }

        public required CameraPerspective? Perspective { get; init; }
    }

    private static IResponse GetCamera(
        [FromUrl] string name,
        GameObjectRepository gameObjects,
        ApiResourceRepository resources
    )
    {
        if (gameObjects.FindOrNull(name) is not { } gameObject)
        {
            return CommonResponse.GameObjectNotFound(name);
        }

        if (resources.ContainerOf(gameObject).GetResource<ISceneCamera>() is not { } sceneCamera)
        {
            return gameObject.GetComponentOrNull<OWCamera>() is { } owCamera
                ? Ok(GetUnityCameraResponse(owCamera))
                : CommonResponse.CameraComponentNotFound(name);
        }

        CameraResponse? response = sceneCamera switch
        {
            PerspectiveSceneCamera { Perspective: var perspective }
                => new() { Type = CameraType.Perspective, Perspective = perspective },

            EquirectSceneCamera => new() { Type = CameraType.Equirect, Perspective = null },

            _ => null,
        };

        response.AssertNotNull();

        return Ok(response);
    }

    private static IResponse GetActiveGamera()
    {
        return Locator.GetActiveCamera().OrNull() switch
        {
            { } camera => Ok(new { Name = camera.name, Camera = GetUnityCameraResponse(camera) }),
            _ => ServiceUnavailable(),
        };
    }

    private static CameraResponse GetUnityCameraResponse(OWCamera owCamera)
    {
        return new CameraResponse
        {
            Type = CameraType.Unity,
            Perspective = owCamera.mainCamera
                is { usePhysicalProperties: true, orthographic: false }
                ? owCamera.GetPerspective()
                : null
        };
    }

    private sealed class PutCameraRequest
    {
        public CameraPerspective? Perspective { get; init; }
    }

    private static IResponse PutCamera(
        [FromUrl] string name,
        [FromBody] PutCameraRequest request,
        GameObjectRepository gameObjects,
        ApiResourceRepository resources
    )
    {
        if (gameObjects.FindOrNull(name) is not { } gameObject)
        {
            return CommonResponse.GameObjectNotFound(name);
        }

        if (request.Perspective is not { } perspective)
        {
            return Ok();
        }

        if (resources.ContainerOf(gameObject).GetResource<ISceneCamera>() is { } sceneCamera)
        {
            if (sceneCamera is not PerspectiveSceneCamera perspectiveCamera)
            {
                return MethodNotAllowed();
            }

            perspectiveCamera.Perspective = perspective;
        }
        else
        {
            if (gameObject.GetComponentOrNull<OWCamera>() is not { } owCamera)
            {
                return CommonResponse.CameraComponentNotFound(name);
            }

            if (owCamera.mainCamera.OrNull() is not { orthographic: false })
            {
                return MethodNotAllowed();
            }

            owCamera.mainCamera.usePhysicalProperties = true;

            owCamera.ApplyPerspective(perspective);
        }

        return Ok();
    }
}
