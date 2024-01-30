namespace SceneRecorder.Infrastructure.DependencyInjection;

internal sealed class FactoryService<T>(Func<T> instanceFactory) : IService<T>
    where T : class
{
    public T GetInstance()
    {
        return instanceFactory.Invoke();
    }
}
