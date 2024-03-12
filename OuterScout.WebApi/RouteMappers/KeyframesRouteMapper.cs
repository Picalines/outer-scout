using Newtonsoft.Json;
using OuterScout.Application.Animation;
using OuterScout.Application.Recording;
using OuterScout.Application.SceneCameras;
using OuterScout.Domain;
using OuterScout.Infrastructure.DependencyInjection;
using OuterScout.Infrastructure.Extensions;
using OuterScout.WebApi.Extensions;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Services;
using UnityEngine;

namespace OuterScout.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class KeyframesRouteMapper : IRouteMapper, IServiceConfiguration
{
    private const string KeyframeScope = "request.keyframes";

    public static KeyframesRouteMapper Instance { get; } = new();

    private KeyframesRouteMapper() { }

    private sealed class KeyframeDTO<T>
    {
        public required T Value { get; init; }
    }

    private sealed class PutKeyframesRequest
    {
        public required string Property { get; init; }

        public required object Keyframes { get; init; }
    }

    private sealed class PutKeyframesRequest<T>
    {
        public required string Property { get; init; }

        public required Dictionary<int, KeyframeDTO<T>> Keyframes { get; init; }
    }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithSceneCreatedFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            serverBuilder.MapPut("scene/keyframes", PutSceneKeyframes);

            serverBuilder.MapPut("cameras/:id/keyframes", PutCameraKeyframes);

            serverBuilder.MapPut("gameObjects/:name/keyframes", PutGameObjectKeyframes);
        }
    }

    public void RegisterServices(ServiceContainer.Builder services)
    {
        using (services.InScope(KeyframeScope))
        {
            services.Register<Lerper<float>>().AsExternalReference(Mathf.Lerp);
            services.Register<Lerper<Vector2>>().AsExternalReference(Vector2.Lerp);
            services.Register<Lerper<Vector3>>().AsExternalReference(Vector3.Lerp);
            services.Register<Lerper<Quaternion>>().AsExternalReference(Quaternion.Slerp);

            services.Register<PutTimeScaleKeyframesHandler>().As<IPutKeyframesHandler>();
            services.Register<PutPositionKeyframesHandler>().As<IPutKeyframesHandler>();
            services.Register<PutRotationKeyframesHandler>().As<IPutKeyframesHandler>();
            services.Register<PutScaleKeyframesHandler>().As<IPutKeyframesHandler>();
            services.Register<PutFocalLengthKeyframesHandler>().As<IPutKeyframesHandler>();
            services.Register<PutSensorSizeKeyframesHandler>().As<IPutKeyframesHandler>();
            services.Register<PutLensShiftKeyframesHandler>().As<IPutKeyframesHandler>();
            services.Register<PutNearClipPlaneKeyframesHandler>().As<IPutKeyframesHandler>();
            services.Register<PutFarClipPlaneKeyframesHandler>().As<IPutKeyframesHandler>();
        }
    }

    private static IResponse PutSceneKeyframes(
        [FromBody] PutKeyframesRequest request,
        ApiResourceRepository apiResources,
        IServiceScope services
    )
    {
        using var keyframeScope = services.StartScope(KeyframeScope);

        var handler = keyframeScope
            .ResolveAll<IPutKeyframesHandler>()
            .FirstOrDefault(h => h.Property == request.Property);

        return handler switch
        {
            IPutKeyframesHandler<Unit> h => h.HandleRequest(Unit.Instance),
            _ => BadRequest($"property '{request.Property}' is not animatable"),
        };
    }

    private static IResponse PutCameraKeyframes(
        [FromUrl] string id,
        [FromBody] PutKeyframesRequest request,
        ApiResourceRepository apiResources,
        IServiceScope services
    )
    {
        if (
            apiResources.GlobalContainer.GetResource<ISceneCamera>(id)
            is not { Transform: { } transform } camera
        )
        {
            return NotFound($"camera '{id}' not found");
        }

        using var keyframeScope = services.StartScope(KeyframeScope);

        var handler = keyframeScope
            .ResolveAll<IPutKeyframesHandler>()
            .FirstOrDefault(h => h.Property == request.Property);

        return handler switch
        {
            IPutKeyframesHandler<PerspectiveSceneCamera> ph
                when camera is PerspectiveSceneCamera perspectiveCamera
                => ph.HandleRequest(perspectiveCamera),

            IPutKeyframesHandler<EquirectSceneCamera> eh
                when camera is EquirectSceneCamera equirectCamera
                => eh.HandleRequest(equirectCamera),

            IPutKeyframesHandler<ISceneCamera> ch => ch.HandleRequest(camera),

            IPutKeyframesHandler<Transform> th => th.HandleRequest(transform),

            _ => BadRequest($"property '{request.Property}' is not animatable"),
        };
    }

    private static IResponse PutGameObjectKeyframes(
        [FromUrl] string name,
        [FromBody] PutKeyframesRequest request,
        GameObjectRepository gameObjects,
        IServiceScope services
    )
    {
        if (gameObjects.FindOrNull(name) is not { } gameObject)
        {
            return NotFound($"gameObject '{name}' not found");
        }

        using var keyframeScope = services.StartScope(KeyframeScope);

        var handler = keyframeScope
            .ResolveAll<IPutKeyframesHandler>()
            .FirstOrDefault(h => h.Property == request.Property);

        return handler switch
        {
            IPutKeyframesHandler<GameObject> gh => gh.HandleRequest(gameObject),
            IPutKeyframesHandler<Transform> th => th.HandleRequest(gameObject.transform),
            _ => BadRequest($"property '{request.Property}' is not animatable"),
        };
    }

    private interface IPutKeyframesHandler
    {
        public string Property { get; }
    }

    private interface IPutKeyframesHandler<E> : IPutKeyframesHandler
    {
        public IResponse HandleRequest(E entity);
    }

    private abstract class PutKeyframesHandler<E, T> : IPutKeyframesHandler<E>
    {
        public string Property { get; }

        public required Request Request { private get; init; }

        public required JsonSerializer JsonSerializer { private get; init; }

        public required Lerper<T> Lerper { private get; init; }

        public required ApiResourceRepository ApiResources { protected get; init; }

        public PutKeyframesHandler(string property)
        {
            Property = property;
        }

        public IResponse HandleRequest(E entity)
        {
            if (
                JsonSerializer.Deserialize<PutKeyframesRequest<T>>(Request.Body)
                is not { Keyframes: var keyframes }
            )
            {
                return BadRequest($"invalid request body");
            }

            var resources = GetContainer(entity);

            if (
                resources.GetResource<Animator<T>>(Property)
                is not { Curve: PropertyCurve<T> propertyCurve } animator
            )
            {
                if (
                    ApiResources.GlobalContainer.GetResource<SceneRecorder.Builder>()
                    is not { } sceneRecorderBuilder
                )
                {
                    return ServiceUnavailable();
                }

                var applier = CreateApplier(entity);
                propertyCurve = new PropertyCurve<T>(Lerper);
                animator = new Animator<T>(propertyCurve, applier);

                resources.AddResource(Property, animator);
                sceneRecorderBuilder.WithAnimator(animator);
            }

            foreach (var (frame, keyframeDto) in keyframes)
            {
                propertyCurve.StoreKeyframe(new Keyframe<T>(frame, keyframeDto.Value));
            }

            return Ok();
        }

        protected abstract IApiResourceContainer GetContainer(E entity);

        protected abstract Applier<T> CreateApplier(E entity);
    }

    private sealed class PutTimeScaleKeyframesHandler()
        : PutKeyframesHandler<Unit, float>("time.scale")
    {
        protected override IApiResourceContainer GetContainer(Unit _) =>
            ApiResources.GlobalContainer;

        protected override Applier<float> CreateApplier(Unit _) =>
            timeScale => Time.timeScale = timeScale;
    }

    private sealed class PutPositionKeyframesHandler()
        : PutKeyframesHandler<Transform, Vector3>("transform.position")
    {
        protected override IApiResourceContainer GetContainer(Transform transform) =>
            ApiResources.ContainerOf(transform.gameObject);

        protected override Applier<Vector3> CreateApplier(Transform transform) =>
            position => transform.localPosition = position;
    }

    private sealed class PutRotationKeyframesHandler()
        : PutKeyframesHandler<Transform, Quaternion>("transform.rotation")
    {
        protected override IApiResourceContainer GetContainer(Transform transform) =>
            ApiResources.ContainerOf(transform.gameObject);

        protected override Applier<Quaternion> CreateApplier(Transform transform) =>
            rotation => transform.localRotation = rotation;
    }

    private sealed class PutScaleKeyframesHandler()
        : PutKeyframesHandler<Transform, Vector3>("transform.scale")
    {
        protected override IApiResourceContainer GetContainer(Transform transform) =>
            ApiResources.ContainerOf(transform.gameObject);

        protected override Applier<Vector3> CreateApplier(Transform transform) =>
            scale => transform.localScale = scale;
    }

    private sealed class PutFocalLengthKeyframesHandler()
        : PutKeyframesHandler<PerspectiveSceneCamera, float>("perspective.focalLength")
    {
        protected override IApiResourceContainer GetContainer(PerspectiveSceneCamera camera) =>
            ApiResources.ContainerOf(camera.transform.gameObject);

        protected override Applier<float> CreateApplier(PerspectiveSceneCamera camera) =>
            focalLength =>
                camera.Perspective = camera.Perspective with { FocalLength = focalLength };
    }

    private sealed class PutSensorSizeKeyframesHandler()
        : PutKeyframesHandler<PerspectiveSceneCamera, Vector2>("perspective.sensorSize")
    {
        protected override IApiResourceContainer GetContainer(PerspectiveSceneCamera camera) =>
            ApiResources.ContainerOf(camera.transform.gameObject);

        protected override Applier<Vector2> CreateApplier(PerspectiveSceneCamera camera) =>
            sensorSize => camera.Perspective = camera.Perspective with { SensorSize = sensorSize };
    }

    private sealed class PutLensShiftKeyframesHandler()
        : PutKeyframesHandler<PerspectiveSceneCamera, Vector2>("perspective.lensShift")
    {
        protected override IApiResourceContainer GetContainer(PerspectiveSceneCamera camera) =>
            ApiResources.ContainerOf(camera.transform.gameObject);

        protected override Applier<Vector2> CreateApplier(PerspectiveSceneCamera camera) =>
            lensShift => camera.Perspective = camera.Perspective with { LensShift = lensShift };
    }

    private sealed class PutNearClipPlaneKeyframesHandler()
        : PutKeyframesHandler<PerspectiveSceneCamera, float>("perspective.nearClipPlane")
    {
        protected override IApiResourceContainer GetContainer(PerspectiveSceneCamera camera) =>
            ApiResources.ContainerOf(camera.transform.gameObject);

        protected override Applier<float> CreateApplier(PerspectiveSceneCamera camera) =>
            nearClipPlane =>
                camera.Perspective = camera.Perspective with { NearClipPlane = nearClipPlane };
    }

    private sealed class PutFarClipPlaneKeyframesHandler()
        : PutKeyframesHandler<PerspectiveSceneCamera, float>("perspective.farClipPlane")
    {
        protected override IApiResourceContainer GetContainer(PerspectiveSceneCamera camera) =>
            ApiResources.ContainerOf(camera.transform.gameObject);

        protected override Applier<float> CreateApplier(PerspectiveSceneCamera camera) =>
            farClipPlane =>
                camera.Perspective = camera.Perspective with { FarClipPlane = farClipPlane };
    }
}
