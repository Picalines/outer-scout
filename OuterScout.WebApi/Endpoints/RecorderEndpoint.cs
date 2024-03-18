using JsonSubTypes;
using Newtonsoft.Json;
using OuterScout.Application.FFmpeg;
using OuterScout.Application.Recording;
using OuterScout.Application.SceneCameras;
using OuterScout.WebApi.Extensions;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Services;
using UnityEngine;

namespace OuterScout.WebApi.Endpoints;

using static ResponseFabric;

internal sealed class RecorderEndpoint : IRouteMapper
{
    public static RecorderEndpoint Instance { get; } = new();

    private RecorderEndpoint() { }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithSceneCreatedFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            serverBuilder.MapPost("cameras/:id/recorders", PostCameraRecorder);

            serverBuilder.MapPost("gameObjects/:name/recorders", PostGameObjectRecorder);
        }
    }

    [JsonConverter(typeof(JsonSubtypes), nameof(IPostRecorderRequest.Property))]
    [JsonSubtypes.KnownSubType(typeof(PostColorTextureRecorderRequest), "renderTexture.color")]
    [JsonSubtypes.KnownSubType(typeof(PostDepthTextureRecorderRequest), "renderTexture.depth")]
    [JsonSubtypes.KnownSubType(typeof(PostTransformRecorderRequest), "transform")]
    private interface IPostRecorderRequest
    {
        public string Property { get; }
    }

    private abstract class PostTextureRecorderRequest : IPostRecorderRequest
    {
        public abstract required string Property { get; init; }

        public required string OutputPath { get; init; }

        public required string Format { get; init; }

        public int ConstantRateFactor { get; init; } = 18;
    }

    private sealed class PostColorTextureRecorderRequest : PostTextureRecorderRequest
    {
        public override required string Property { get; init; } = "renderTexture.color";
    }

    private sealed class PostDepthTextureRecorderRequest : PostTextureRecorderRequest
    {
        public override required string Property { get; init; } = "renderTexture.depth";
    }

    private sealed class PostTransformRecorderRequest : IPostRecorderRequest
    {
        public required string Property { get; init; } = "transform";

        public required string OutputPath { get; init; }

        public required string Format { get; init; }
    }

    private static IResponse PostCameraRecorder(
        [FromUrl] string id,
        [FromBody] IPostRecorderRequest request,
        JsonSerializer jsonSerializer,
        ApiResourceRepository apiResources
    )
    {
        if (
            apiResources.GlobalContainer.GetResource<ISceneCamera>(id)
            is not { Transform.gameObject: var gameObject } camera
        )
        {
            return CommonResponse.CameraNotFound(id);
        }

        return PostRecorder(
            apiResources,
            jsonSerializer,
            request,
            apiResources.ContainerOf(gameObject),
            camera
        );
    }

    private static IResponse PostGameObjectRecorder(
        [FromUrl] string name,
        [FromBody] IPostRecorderRequest request,
        JsonSerializer jsonSerializer,
        ApiResourceRepository apiResources,
        GameObjectRepository gameObjects
    )
    {
        if (gameObjects.FindOrNull(name) is not { } gameObject)
        {
            return CommonResponse.GameObjectNotFound(name);
        }

        return PostRecorder(
            apiResources,
            jsonSerializer,
            request,
            apiResources.ContainerOf(gameObject),
            gameObject
        );
    }

    private static IResponse PostRecorder<E>(
        ApiResourceRepository apiResources,
        JsonSerializer jsonSerializer,
        IPostRecorderRequest request,
        IApiResourceContainer container,
        E entity
    )
    {
        if (container.GetResource<IRecorder.IBuilder>(request.Property) is { })
        {
            return BadRequest($"recorder for property '{request.Property}' already exists");
        }

        var (response, recorder) = (request, entity) switch
        {
            (PostTextureRecorderRequest textureRecorderRequest, ISceneCamera camera)
                => PostRenderTextureRecorder(textureRecorderRequest, camera),

            (
                PostTransformRecorderRequest transformRecorderRequest,
                GameObject { transform: var transform }
            )
                => PostTransformRecorder(
                    apiResources,
                    jsonSerializer,
                    transformRecorderRequest,
                    transform
                ),

            _ => (BadRequest($"property '{request.Property}' is not recordable"), null)
        };

        if (recorder is not null)
        {
            container.AddResource(request.Property, recorder);
        }

        return response;
    }

    private static (IResponse, IRecorder.IBuilder?) PostRenderTextureRecorder(
        PostTextureRecorderRequest request,
        ISceneCamera camera
    )
    {
        if (FFmpeg.CheckInstallation() is { } exception)
        {
            return (
                ServiceUnavailable(
                    new { Error = "ffmpeg is not available", Exception = exception }
                ),
                null
            );
        }

        if (request.Format is not "mp4")
        {
            return (BadRequest($"format '{request.Format}' is not supported"), null);
        }

        if (request.ConstantRateFactor is < 0 or > 63)
        {
            return (BadRequest("unsupported constant rate factor value"), null);
        }

        var renderTexture = request switch
        {
            PostColorTextureRecorderRequest => camera.ColorTexture,
            PostDepthTextureRecorderRequest => camera.DepthTexture,
            _ => null,
        };

        if (renderTexture is null)
        {
            return (BadRequest($"camera cannot record {request.Property}"), null);
        }

        var recorder = new RenderTextureRecorder.Builder(
            request.OutputPath,
            renderTexture
        ).WithConstantRateFactor(request.ConstantRateFactor);

        return (Created(), recorder);
    }

    private static (IResponse, IRecorder.IBuilder?) PostTransformRecorder(
        ApiResourceRepository apiResources,
        JsonSerializer jsonSerializer,
        PostTransformRecorderRequest request,
        Transform transform
    )
    {
        if (request.Format is not "json")
        {
            return (BadRequest($"format '{request.Format}' is not supported"), null);
        }

        var origin = apiResources
            .GlobalContainer.GetRequiredResource<GameObject>(SceneEndpoint.OriginResource)
            .transform;

        var transformGetter = () =>
        {
            if (transform == null)
            {
                return null;
            }

            return origin.InverseDto(transform);
        };

        var recorder = new JsonRecorder.Builder(request.OutputPath, transformGetter);
        recorder.WithJsonSerializer(jsonSerializer);

        return (Created(), recorder);
    }
}
