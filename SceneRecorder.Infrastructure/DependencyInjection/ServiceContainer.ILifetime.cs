using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    public interface ILifetime<out T>
        where T : class
    {
        public T GetInstance();
    }

    public interface IStartupHandler
    {
        public void OnContainerStartup(ServiceContainer container);
    }

    public sealed class ReferenceLifetime<T> : ILifetime<T>, IStartupHandler, IDisposable
        where T : class
    {
        private T? _instance;

        public ReferenceLifetime(T instance)
        {
            _instance = instance;
        }

        public T GetInstance()
        {
            _instance.ThrowIfNull();
            return _instance;
        }

        void IStartupHandler.OnContainerStartup(ServiceContainer container)
        {
            if (_instance is IStartupHandler startupHandler)
            {
                startupHandler.OnContainerStartup(container);
            }
        }

        void IDisposable.Dispose()
        {
            _instance = null;
        }
    }

    public sealed class SingletonLifetime<T> : ILifetime<T>, IStartupHandler, IDisposable
        where T : class
    {
        private T? _instance;

        public T GetInstance()
        {
            _instance.ThrowIfNull();
            return _instance;
        }

        void IStartupHandler.OnContainerStartup(ServiceContainer container)
        {
            _instance = container.Resolve<IInstantiator<T>>().Instantiate();

            if (_instance is IStartupHandler startupHandler)
            {
                startupHandler.OnContainerStartup(container);
            }
        }

        void IDisposable.Dispose()
        {
            (_instance as IDisposable)?.Dispose();
            _instance = null;
        }
    }
}
