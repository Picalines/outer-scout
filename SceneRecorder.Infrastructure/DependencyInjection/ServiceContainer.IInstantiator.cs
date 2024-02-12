namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    public interface IInstantiator<out T>
        where T : class
    {
        public T Instantiate();
    }

    private sealed class DefaultConstructorInstantiator<T> : IInstantiator<T>
        where T : class, new()
    {
        public T Instantiate()
        {
            return new T();
        }
    }

    private sealed class FactoryInstantiator<T>(Func<T> instantiate) : IInstantiator<T>
        where T : class
    {
        public T Instantiate()
        {
            return instantiate.Invoke();
        }
    }
}
