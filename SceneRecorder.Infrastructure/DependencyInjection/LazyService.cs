namespace SceneRecorder.Infrastructure.DependencyInjection;

internal sealed class LazyService<T>(Lazy<T> lazyInstance) : IService<T>, IDisposable
    where T : class
{
    public T GetInstance()
    {
        return lazyInstance.Value;
    }

    public void Dispose()
    {
        if (lazyInstance.IsValueCreated)
        {
            (lazyInstance.Value as IDisposable)?.Dispose();
        }
    }
}
