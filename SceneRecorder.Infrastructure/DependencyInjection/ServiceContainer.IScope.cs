using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    public interface IScope : IServiceContainer, IDisposable
    {
        public IScope StartScope(string identifier);
    }

    private sealed class Scope : IScope
    {
        private readonly string? _identifier;

        private readonly Scope? _parent;

        private readonly IScopeRegistry _scopeRegistry;

        private readonly IServiceRegistry _serviceRegistry;

        private readonly HashSet<IStartupHandler> _lifetimesToInitialize = [];

        private readonly LinkedList<ICleanupHandler> _lifetimesToCleanup = [];

        private readonly HashSet<Scope> _childScopes = [];

        private bool _disposed = false;

        public Scope(IScopeRegistry scopeRegistry, IServiceRegistry globalServiceRegistry)
        {
            _identifier = null;
            _parent = null;
            _scopeRegistry = scopeRegistry;
            _serviceRegistry = globalServiceRegistry;

            InitializeServices();
        }

        private Scope(
            string identifier,
            Scope parent,
            IScopeRegistry scopeRegistry,
            IServiceRegistry serviceRegistry
        )
        {
            _identifier = identifier;
            _parent = parent;
            _scopeRegistry = scopeRegistry;
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

            foreach (var childScope in _childScopes)
            {
                childScope.Dispose();
            }

            while (_lifetimesToCleanup.FirstOrDefault() is { } cleanupHandler)
            {
                _lifetimesToCleanup.RemoveFirst();
                cleanupHandler.CleanupService();
            }

            _identifier.ThrowIfNull();
            _scopeRegistry.DeactivateScopeOrThrow(_identifier);
        }

        public IScope StartScope(string identifier)
        {
            AssertNotDisposed();

            var childServiceRegistry = _scopeRegistry.ActivateScopeOrThrow(identifier);

            var childScope = new Scope(identifier, this, _scopeRegistry, childServiceRegistry);

            _childScopes.Add(childScope);

            return childScope;
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
