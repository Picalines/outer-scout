using Newtonsoft.Json;
using SceneRecorder.Application.FFmpeg;
using SceneRecorder.Application.SceneCameras;
using SceneRecorder.WebApi.DTOs;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using SceneRecorder.WebApi.Services;

namespace SceneRecorder.WebApi.RouteMappers;

using SceneRecorder.Application.Recording;
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
        string cameraId,
        string textureType,
        [FromBody] CreateTextureRecorderRequest request,
        SceneRecorder.Builder sceneRecorderBuilder
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

        if (ApiResource.Find<ISceneCamera>(cameraId) is not { Value: var camera })
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
        string gameObjectName,
        [FromBody] CreateTransformRecorderRequest request,
        SceneRecorder.Builder sceneRecorderBuilder,
        JsonSerializer jsonSerializer,
        GameObjectRepository gameObjects
    )
    {
        if (request.OutputPath.EndsWith(".json") is false)
        {
            return BadRequest("only .json output is supported");
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
