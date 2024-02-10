using SceneRecorder.Infrastructure.DependencyInjection;
using UnityEngine;

namespace SceneRecorder.Domain;

public sealed class SceneService<T>(Func<T> instanceFactory) : IService<T>, IDisposable
    where T : class
{
    private GameObject? _gameObject = null;

    private ISceneResource<T>? _resource = null;

    public T GetInstance()
    {
        if (_resource is not { IsAccessable: true })
        {
            var resourceValue = instanceFactory();

            _gameObject = new GameObject($"{nameof(SceneRecorder)}.{nameof(SceneService<T>)}");

            _resource = _gameObject.AddResource(resourceValue);
        }

        return _resource.Value;
    }

    public void Dispose()
    {
        if (_resource is { IsAccessable: true })
        {
            _resource.Dispose();
            UnityEngine.Object.Destroy(_gameObject);
        }
    }
}

public static class SceneServiceExtensions
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
