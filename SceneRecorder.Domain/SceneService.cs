using SceneRecorder.Infrastructure.DependencyInjection;

namespace SceneRecorder.Domain;

public sealed class SceneService<T>(Func<T> instanceFactory) : IService<T>, IDisposable
{
    private SceneResource<T>? _resource = null;

    public T GetInstance()
    {
        if (_resource is not { IsAccessable: true })
        {
            _resource = SceneResource<T>.CreateGlobal(instanceFactory());
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
