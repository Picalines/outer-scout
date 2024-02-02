using SceneRecorder.Infrastructure.DependencyInjection;
using UnityEngine;

namespace SceneRecorder.WebApi.Components;

internal sealed class SceneService<T>(Func<T> instanceFactory) : IService<T>, IDisposable
{
    private ApiResource<T>? _resource = null;

    public T GetInstance()
    {
        if (_resource is not { IsAccessable: true })
        {
            var instance = instanceFactory();

            var gameObject = new GameObject($"{nameof(SceneRecorder)}.{nameof(SceneService<T>)}");

            _resource = gameObject.AddComponent<ApiResource<T>, T>(instance);
        }

        return _resource.Value;
    }

    public void Dispose()
    {
        if (_resource is { IsAccessable: true } and IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

internal static class SceneServiceExtensions
{
    public static IDisposable RegisterSceneInstance<T>(
        this ServiceContainer services,
        Func<T> instanceFactory
    )
        where T : class
    {
        return services.RegisterService(new SceneService<T>(instanceFactory));
    }
}
