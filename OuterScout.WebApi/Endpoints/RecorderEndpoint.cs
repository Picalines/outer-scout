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
    public static class PropertyName
    {
        public const string ColorTexture = "camera.renderTexture.color";

        public const string DepthTexture = "camera.renderTexture.depth";

        public const string Transform = "transform";
    }

    public static RecorderEndpoint Instance { get; } = new();

    private RecorderEndpoint() { }

    void IRouteMapper.MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithSceneCreatedFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            serverBuilder.MapPost("objects/:name/recorders", PostGameObjectRecorder);
        }
    }

    [JsonConverter(typeof(JsonSubtypes), nameof(IPostRecorderRequest.Property))]
    [JsonSubtypes.KnownSubType(typeof(PostColorTextureRecorderRequest), PropertyName.ColorTexture)]
    [JsonSubtypes.KnownSubType(typeof(PostDepthTextureRecorderRequest), PropertyName.DepthTexture)]
    [JsonSubtypes.KnownSubType(typeof(PostTransformRecorderRequest), PropertyName.Transform)]
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
        public override required string Property { get; init; } = PropertyName.ColorTexture;
    }

    private sealed class PostDepthTextureRecorderRequest : PostTextureRecorderRequest
    {
        public override required string Property { get; init; } = PropertyName.DepthTexture;
    }

    private sealed class PostTransformRecorderRequest : IPostRecorderRequest
    {
        public required string Property { get; init; } = PropertyName.Transform;

        public required string OutputPath { get; init; }

        public required string Format { get; init; }

        public string? Origin { get; init; }
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

        var container = apiResources.ContainerOf(gameObject);

        if (container.GetResource<IRecorder.IBuilder>(request.Property) is { })
        {
            return BadRequest($"recorder for property '{request.Property}' already exists");
        }

        var (response, recorder) = request switch
        {
            PostTextureRecorderRequest textureRecorderRequest
                => PostRenderTextureRecorder(apiResources, textureRecorderRequest, gameObject),

            PostTransformRecorderRequest transformRecorderRequest
                => PostTransformRecorder(
                    apiResources,
                    gameObjects,
                    jsonSerializer,
                    transformRecorderRequest,
                    gameObject
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
        ApiResourceRepository apiResources,
        PostTextureRecorderRequest request,
        GameObject gameObject
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

        if (apiResources.ContainerOf(gameObject).GetResource<ISceneCamera>() is not { } camera)
        {
            return (CommonResponse.CameraComponentNotFound(gameObject.name), null);
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
        GameObjectRepository gameObjects,
        JsonSerializer jsonSerializer,
        PostTransformRecorderRequest request,
        GameObject gameObject
    )
    {
        if (request.Format is not "json")
        {
            return (BadRequest($"format '{request.Format}' is not supported"), null);
        }

        var (origin, originName) = request switch
        {
            { Origin: { } customOriginName }
                => (gameObjects.FindOrNull(customOriginName)?.transform, customOriginName),
            { Origin: null }
                => (SceneEndpoint.GetOriginOrNull(gameObjects), SceneEndpoint.OriginResource),
        };

        if (origin is null)
        {
            return (CommonResponse.GameObjectNotFound(originName), null);
        }

        var transform = gameObject.transform;

        var transformGetter = () =>
        {
            if (transform == null || origin == null)
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
