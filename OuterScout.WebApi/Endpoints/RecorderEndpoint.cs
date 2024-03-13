using Newtonsoft.Json;
using OuterScout.Application.FFmpeg;
using OuterScout.Application.Recording;
using OuterScout.Application.SceneCameras;
using OuterScout.Infrastructure.DependencyInjection;
using OuterScout.Infrastructure.Extensions;
using OuterScout.Infrastructure.Validation;
using OuterScout.WebApi.DTOs;
using OuterScout.WebApi.Extensions;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Services;
using UnityEngine;

namespace OuterScout.WebApi.Endpoints;

using static ResponseFabric;

internal sealed class RecorderEndpoint : IRouteMapper, IServiceConfiguration
{
    private const string RecorderScope = "request.recorder";

    public static RecorderEndpoint Instance { get; } = new();

    private RecorderEndpoint() { }

    private class BaseRequestBody
    {
        public required string Property { get; init; }
    }

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

    public void RegisterServices(ServiceContainer.Builder services)
    {
        using (services.InScope(RecorderScope))
        {
            services.Register<PostTransformRecorderHandler>().As<IPostRecorderHandler>();
            services.Register<PostColorTextureRecorderHandler>().As<IPostRecorderHandler>();
            services.Register<PostDepthTextureRecorderHandler>().As<IPostRecorderHandler>();
        }
    }

    private static IResponse PostCameraRecorder(
        [FromUrl] string id,
        Request request,
        JsonSerializer jsonSerializer,
        ApiResourceRepository resources,
        IServiceScope services
    )
    {
        if (resources.GlobalContainer.GetResource<ISceneCamera>(id) is not { } camera)
        {
            return CommonResponse.CameraNotFound(id);
        }

        var property = ReadPropertyFromBody(request, jsonSerializer);

        using var recorderScope = services.StartScope(RecorderScope);

        var handler = recorderScope
            .ResolveAll<IPostRecorderHandler>()
            .FirstOrDefault(h => h.Property == property);

        return handler switch
        {
            IPostRecorderHandler<ISceneCamera> ch => ch.HandleRequest(camera),
            _ => BadRequest($"property '{property}' is not recordable"),
        };
    }

    private static IResponse PostGameObjectRecorder(
        [FromUrl] string name,
        Request request,
        JsonSerializer jsonSerializer,
        GameObjectRepository gameObjects,
        IServiceScope services
    )
    {
        if (gameObjects.FindOrNull(name) is not { } gameObject)
        {
            return CommonResponse.GameObjectNotFound(name);
        }

        var property = ReadPropertyFromBody(request, jsonSerializer);

        using var recorderScope = services.StartScope(RecorderScope);

        var handler = recorderScope
            .ResolveAll<IPostRecorderHandler>()
            .FirstOrDefault(h => h.Property == property);

        return handler switch
        {
            IPostRecorderHandler<GameObject> gh => gh.HandleRequest(gameObject),
            IPostRecorderHandler<Transform> th => th.HandleRequest(gameObject.transform),
            _ => BadRequest($"property '{property}' is not recordable"),
        };
    }

    private static string ReadPropertyFromBody(Request request, JsonSerializer jsonSerializer)
    {
        var requestBase = jsonSerializer.Deserialize<BaseRequestBody>(request.BodyReader);
        requestBase.AssertNotNull();

        request.BodyReader.BaseStream.Position = 0;

        return requestBase.Property;
    }

    private interface IPostRecorderHandler
    {
        public string Property { get; }
    }

    private interface IPostRecorderHandler<E> : IPostRecorderHandler
    {
        public IResponse HandleRequest(E entity);
    }

    private abstract class PostRecorderHandler<E, B> : IPostRecorderHandler<E>
        where B : BaseRequestBody
    {
        public string Property { get; }

        public required Request Request { protected get; init; }

        public required JsonSerializer JsonSerializer { protected get; init; }

        public required ApiResourceRepository ApiResources { protected get; init; }

        public PostRecorderHandler(string property)
        {
            Property = property;
        }

        public IResponse HandleRequest(E entity)
        {
            var requestBody = JsonSerializer.Deserialize<B>(Request.BodyReader);
            requestBody.AssertNotNull();

            var (response, recorder) = CreateRecorder(entity, requestBody);

            if (recorder is not null)
            {
                ApiResources
                    .GlobalContainer.GetRequiredResource<SceneRecorder.Builder>()
                    .WithRecorder(recorder);
            }

            return response;
        }

        protected abstract (IResponse, IRecorder.IBuilder?) CreateRecorder(E entity, B requestBody);
    }

    private sealed class CreateTextureRecorderRequest : BaseRequestBody
    {
        public required string OutputPath { get; init; }

        public required string Format { get; init; }

        public int ConstantRateFactor { get; init; } = 18;
    }

    private abstract class PostRenderTextureRecorderHandler(string property)
        : PostRecorderHandler<ISceneCamera, CreateTextureRecorderRequest>(property)
    {
        protected sealed override (IResponse, IRecorder.IBuilder?) CreateRecorder(
            ISceneCamera camera,
            CreateTextureRecorderRequest requestBody
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

            if (requestBody.Format is not "mp4")
            {
                return (BadRequest($"format '{requestBody.Format}' is not supported"), null);
            }

            if (requestBody.ConstantRateFactor is < 0 or > 63)
            {
                return (BadRequest("unsupported constant rate factor value"), null);
            }

            if (GetRenderTexture(camera) is not { } renderTexture)
            {
                return (BadRequest($"camera cannot record {Property}"), null);
            }

            var recorder = new RenderTextureRecorder.Builder(
                requestBody.OutputPath,
                renderTexture
            ).WithConstantRateFactor(requestBody.ConstantRateFactor);

            return (Created(), recorder);
        }

        protected abstract RenderTexture? GetRenderTexture(ISceneCamera camera);
    }

    private sealed class PostColorTextureRecorderHandler()
        : PostRenderTextureRecorderHandler("renderTexture.color")
    {
        protected override RenderTexture? GetRenderTexture(ISceneCamera camera) =>
            camera.ColorTexture;
    }

    private sealed class PostDepthTextureRecorderHandler()
        : PostRenderTextureRecorderHandler("renderTexture.depth")
    {
        protected override RenderTexture? GetRenderTexture(ISceneCamera camera) =>
            camera.DepthTexture;
    }

    private sealed class CreateTransformRecorderRequest : BaseRequestBody
    {
        public required string OutputPath { get; init; }

        public required string Format { get; init; }
    }

    private sealed class PostTransformRecorderHandler()
        : PostRecorderHandler<Transform, CreateTransformRecorderRequest>("transform")
    {
        public required GameObjectRepository GameObjects { private get; init; }

        protected override (IResponse, IRecorder.IBuilder?) CreateRecorder(
            Transform transform,
            CreateTransformRecorderRequest requestBody
        )
        {
            if (requestBody.Format is not "json")
            {
                return (BadRequest($"format '{requestBody.Format}' is not supported"), null);
            }

            var origin = ApiResources
                .GlobalContainer.GetRequiredResource<GameObject>(SceneEndpoint.OriginResource)
                .transform;

            var transformGetter = () =>
            {
                if (transform == null)
                {
                    return null;
                }

                return new TransformDTO()
                {
                    Position = origin.InverseTransformPoint(transform.position),
                    Rotation = origin.InverseTransformRotation(transform.rotation),
                    Scale = transform.lossyScale,
                };
            };

            var recorder = new JsonRecorder.Builder(
                requestBody.OutputPath,
                transformGetter
            ).WithJsonSerializer(JsonSerializer);

            return (Created(), recorder);
        }
    }
}
