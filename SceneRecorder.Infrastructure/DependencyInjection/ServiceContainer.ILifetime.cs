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
        public void InitializeService(IContainer container);
    }

    public interface ICleanupHandler
    {
        public void CleanupService();
    }

    public sealed class ReferenceLifetime<T> : ILifetime<T>, IStartupHandler, ICleanupHandler
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

        void IStartupHandler.InitializeService(IContainer container)
        {
            if (_instance is IStartupHandler startupHandler)
            {
                startupHandler.InitializeService(container);
            }
        }

        void ICleanupHandler.CleanupService()
        {
            _instance = null;
        }
    }

    public sealed class SingletonLifetime<T> : ILifetime<T>, IStartupHandler, ICleanupHandler
        where T : class
    {
        private T? _instance;

        T ILifetime<T>.GetInstance()
        {
            _instance.ThrowIfNull();
            return _instance;
        }

        void IStartupHandler.InitializeService(IContainer container)
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
