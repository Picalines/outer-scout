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
        public void InitializeService(IServiceContainer container);
    }

    public interface ICleanupHandler
    {
        public void CleanupService();
    }

    internal sealed class ReferenceLifetime<T> : ILifetime<T>, IStartupHandler, IDisposable
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

        void IStartupHandler.InitializeService(IServiceContainer container)
        {
            if (_instance is IStartupHandler startupHandler)
            {
                startupHandler.InitializeService(container);
            }
        }

        void IDisposable.Dispose()
        {
            _instance = null;
        }
    }

    internal sealed class SingletonLifetime<T> : ILifetime<T>, IStartupHandler, ICleanupHandler
        where T : class
    {
        private T? _instance;

        T ILifetime<T>.GetInstance()
        {
            _instance.ThrowIfNull();
            return _instance;
        }

        void IStartupHandler.InitializeService(IServiceContainer container)
        {
            _instance = container.Resolve<IInstantiator<T>>().Instantiate();

            if (_instance is IStartupHandler startupHandler)
            {
                startupHandler.InitializeService(container);
            }
        }

        void ICleanupHandler.CleanupService()
        {
            (_instance as IDisposable)?.Dispose();
            _instance = null;
        }
    }
}
