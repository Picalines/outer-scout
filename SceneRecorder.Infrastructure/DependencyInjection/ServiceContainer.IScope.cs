namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    public interface IContainer : IDisposable
    {
        public bool Contains(Type type);

        public object? ResolveOrNull(Type type);

        public object Resolve(Type type);

        public bool Contains<T>()
            where T : class;

        public T? ResolveOrNull<T>()
            where T : class;

        public T Resolve<T>()
            where T : class;
    }

    public interface IScope : IContainer, IDisposable
    {
        public IScope StartScope(string identifier);
    }

    private sealed class Scope : IScope
    {
        private readonly Scope? _parent;

        private readonly ServiceRegistry _serviceRegistry;

        private readonly HashSet<IStartupHandler> _lifetimesToInitialize = [];

        private readonly LinkedList<ICleanupHandler> _lifetimesToCleanup = [];

        private bool _disposed = false;

        public Scope(Scope? parent, ServiceRegistry serviceRegistry)
        {
            _parent = parent;
            _serviceRegistry = serviceRegistry;

            InitializeServices();
        }

        public object? ResolveOrNull(Type serviceType)
        {
            return ResolveLifetime(serviceType)?.GetInstance();
        }

        public object Resolve(Type serviceType)
        {
            return ResolveOrNull(serviceType)
                ?? throw new InvalidOperationException(
                    $"{nameof(ServiceContainer)} does not contain service of type {serviceType}"
                );
        }

        public T? ResolveOrNull<T>()
            where T : class
        {
            return ResolveOrNull(typeof(T)) as T;
        }

        public T Resolve<T>()
            where T : class
        {
            return (Resolve(typeof(T)) as T)!;
        }

        public bool Contains(Type type)
        {
            return _serviceRegistry.ContainsService(type) || (_parent?.Contains(type) is true);
        }

        public bool Contains<T>()
            where T : class
        {
            return Contains(typeof(T));
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            while (_lifetimesToCleanup.FirstOrDefault() is { } cleanupHandler)
            {
                _lifetimesToCleanup.RemoveFirst();
                cleanupHandler.CleanupService();
            }

            _serviceRegistry.Dispose();
        }

        public IScope StartScope(string identifier)
        {
            // TODO
            throw new NotImplementedException();
        }

        // Remember that one service with IStartupHandler can Resolve
        // other service that also must be initialized - that's
        // why we call InitializeService during the resolution
        private ILifetime<object>? ResolveLifetime(Type type)
        {
            AssertNotDisposed();

            if (GetLocalServiceLifetime(type) is not { } lifetime)
            {
                return _parent?.ResolveLifetime(type);
            }

            if (lifetime is IStartupHandler startupHandler)
            {
                InitializeService(startupHandler);
            }

            return lifetime;
        }

        private ILifetime<object>? GetLocalServiceLifetime(Type type)
        {
            AssertNotDisposed();

            return _serviceRegistry
                .GetMatchingLifetimes(type)
                .Where(lifetime =>
                    lifetime is not IStartupHandler startupHandler
                    || !_lifetimesToInitialize.Contains(startupHandler)
                )
                .LastOrDefault();
        }

        private void InitializeServices()
        {
            foreach (var lifetime in _serviceRegistry.AllLifetimes)
            {
                if (lifetime is IStartupHandler startupHandler)
                {
                    _lifetimesToInitialize.Add(startupHandler);
                }
                else if (lifetime is ICleanupHandler cleanupHandler)
                {
                    // lifetimes with IStartupHandler also can implement IDisposable,
                    // but then order of their disposal matters. They're added
                    // to _lifetimesToDispose in InitializeService
                    _lifetimesToCleanup.AddLast(cleanupHandler);
                }
            }

            while (_lifetimesToInitialize.FirstOrDefault() is { } service)
            {
                InitializeService(service);
            }
        }

        private void InitializeService(IStartupHandler service)
        {
            if (_lifetimesToInitialize.Contains(service) is false)
            {
                return;
            }

            _lifetimesToInitialize.Remove(service);
            service.InitializeService(this);

            if (service is ICleanupHandler cleanupHandler)
            {
                _lifetimesToCleanup.AddFirst(cleanupHandler);
            }
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new InvalidOperationException($"{nameof(ServiceContainer)} is disposed");
            }
        }
    }
}
