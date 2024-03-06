using Newtonsoft.Json;
using OuterScout.Application.FFmpeg;
using OuterScout.Application.Recording;
using OuterScout.Application.SceneCameras;
using OuterScout.WebApi.DTOs;
using OuterScout.WebApi.Extensions;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Services;

namespace OuterScout.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class RecorderRouteMapper : IRouteMapper
{
    public static RecorderRouteMapper Instance { get; } = new();

    private RecorderRouteMapper() { }

    private sealed class CreateTextureRecorderRequest
    {
        public required string OutputPath { get; init; }

        public int ConstantRateFactor { get; init; } = 18;
    }

    private sealed class CreateTransformRecorderRequest
    {
        public required string OutputPath { get; init; }

        public required string Format { get; init; }

        public required string Parent { get; init; }
    }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithSceneCreatedFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            serverBuilder.MapPost(
                "cameras/:cameraId/textures/:textureType/recorder",
                CreateCameraTextureRecorder
            );

            serverBuilder.MapPost(
                "gameObjects/:gameObjectName/transform/recorder",
                CreateTransformRecorder
            );
        }
    }

    private static IResponse CreateCameraTextureRecorder(
        [FromUrl] string cameraId,
        [FromUrl] string textureType,
        [FromBody] CreateTextureRecorderRequest request,
        ApiResourceRepository resources
    )
    {
        if (FFmpeg.CheckInstallation() is { } exception)
        {
            return ServiceUnavailable(
                new { Error = "ffmpeg is not available", Exception = exception }
            );
        }

        if (request.OutputPath.EndsWith(".mp4") is false)
        {
            return BadRequest("only .mp4 video output is supported");
        }

        if (request.ConstantRateFactor is < 0 or > 63)
        {
            return BadRequest("unsupported constant rate factor value");
        }

        if (
            resources.GlobalContainer.GetResource<SceneRecorder.Builder>()
            is not { } sceneRecorderBuilder
        )
        {
            return ServiceUnavailable();
        }

        if (resources.GlobalContainer.GetResource<ISceneCamera>(cameraId) is not { } camera)
        {
            return NotFound($"camera '{cameraId}' not found");
        }

        var renderTexture = textureType switch
        {
            "color" => camera.ColorTexture,
            "depth" => camera.DepthTexture,
            _ => null,
        };

        if (renderTexture is null)
        {
            return NotFound($"camera '{cameraId}' cannot record {textureType} texture");
        }

        sceneRecorderBuilder.WithRecorder(
            new RenderTextureRecorder.Builder(targetFile: request.OutputPath, renderTexture)
                .WithFrameRate(sceneRecorderBuilder.CaptureFrameRate)
                .WithConstantRateFactor(request.ConstantRateFactor)
        );

        return Ok();
    }

    private static IResponse CreateTransformRecorder(
        [FromUrl] string gameObjectName,
        [FromBody] CreateTransformRecorderRequest request,
        JsonSerializer jsonSerializer,
        GameObjectRepository gameObjects,
        ApiResourceRepository resources
    )
    {
        if (request.Format is not "json")
        {
            return BadRequest("only .json output is supported");
        }

        if (
            resources.GlobalContainer.GetResource<SceneRecorder.Builder>()
            is not { } sceneRecorderBuilder
        )
        {
            return ServiceUnavailable();
        }

        if (gameObjects.FindOrNull(gameObjectName) is not { transform: var targetTransform })
        {
            return NotFound($"gameObject '{gameObjectName}' not found");
        }

        if (gameObjects.FindOrNull(request.Parent) is not { transform: var parentTransform })
        {
            return BadRequest($"gameObject '{request.Parent}' not found");
        }

        var transformGetter = () =>
        {
            if (targetTransform == null || parentTransform == null)
            {
                return null;
            }

            return new TransformDTO()
            {
                Position = parentTransform.InverseTransformPoint(targetTransform.position),
                Rotation = parentTransform.InverseTransformRotation(targetTransform.rotation),
                Scale = targetTransform.lossyScale,
            };
        };

        sceneRecorderBuilder.WithRecorder(
            new JsonRecorder.Builder(targetFile: request.OutputPath, transformGetter)
                .WithJsonSerializer(jsonSerializer)
                .WithAdditionalProperty("parent", request.Parent)
        );

        return Ok();
    }
}
