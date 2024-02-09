namespace SceneRecorder.Infrastructure.DependencyInjection;

internal sealed class SingletonService<T>(T instance) : IService<T>
    where T : class
{
    public T GetInstance()
    {
        return instance;
    }
}
