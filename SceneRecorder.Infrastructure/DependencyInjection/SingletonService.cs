namespace SceneRecorder.Infrastructure.DependencyInjection;

internal sealed class SingletonService<T>(T instance) : IService<T>, IDisposable
    where T : class
{
    public T GetInstance()
    {
        return instance;
    }

    public void Dispose()
    {
        (instance as IDisposable)?.Dispose();
    }
}
