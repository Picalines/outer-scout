using SceneRecorder.Application.SceneCameras;
using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;

namespace SceneRecorder.WebApi.RouteMappers;

using Newtonsoft.Json;
using SceneRecorder.Application.Recording;
using SceneRecorder.WebApi.DTOs;
using UnityEngine;
using static ResponseFabric;

internal sealed class RecorderRouteMapper : IRouteMapper
{
    public static RecorderRouteMapper Instance { get; } = new();

    private RecorderRouteMapper() { }

    private sealed class CreateTextureRecorderRequest
    {
        public required string OutputPath { get; init; }
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
        if (request.OutputPath.EndsWith(".mp4") is false)
        {
            return BadRequest("only .mp4 video output is supported");
        }

        if (SceneResource.Find<ISceneCamera>(cameraId) is not { Value: var camera })
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
            () =>
                RenderTextureRecorder.StartRecording(
                    new()
                    {
                        TargetFile = request.OutputPath,
                        Texture = renderTexture,
                        FrameRate = sceneRecorderBuilder.CaptureFrameRate,
                    }
                )
        );

        return Ok();
    }

    private static IResponse CreateTransformRecorder(
        string gameObjectName,
        [FromBody] CreateTransformRecorderRequest request,
        SceneRecorder.Builder sceneRecorderBuilder,
        JsonSerializer jsonSerializer
    )
    {
        if (request.OutputPath.EndsWith(".json") is false)
        {
            return BadRequest("only .json output is supported");
        }

        if (GameObject.Find(gameObjectName).OrNull() is not { transform: var targetTransform })
        {
            return NotFound($"gameObject '{gameObjectName}' not found");
        }

        if (GameObject.Find(request.Parent).OrNull() is not { transform: var parentTransform })
        {
            return BadRequest($"gameObject '{request.Parent}' not found");
        }

        sceneRecorderBuilder.WithRecorder(
            () =>
                JsonRecorder.StartRecording(
                    new()
                    {
                        TargetFile = request.OutputPath,
                        JsonSerializer = jsonSerializer,
                        AdditionalProperties = { ["parent"] = request.Parent },
                        ValueGetter = () =>
                            new TransformDTO()
                            {
                                Position = parentTransform.InverseTransformPoint(
                                    targetTransform.position
                                ),
                                Rotation = parentTransform.InverseTransformRotation(
                                    targetTransform.rotation
                                ),
                                Scale = targetTransform.lossyScale,
                            },
                    }
                )
        );

        return Ok();
    }
}
