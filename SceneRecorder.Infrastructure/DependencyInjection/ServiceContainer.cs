namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer : IDisposable
{
    private readonly IReadOnlyDictionary<Type, ILifetime<object>> _lifetimes;

    private readonly IReadOnlyDictionary<Type, IEnumerable<Type>> _interfaces;

    private readonly HashSet<IStartupHandler> _lifetimesToInitialize;

    private readonly LinkedList<IDisposable> _lifetimesToDispose = [];

    private readonly Stack<object?> _scopeStack = [];

    private bool _disposed = false;

    private ServiceContainer(
        IReadOnlyDictionary<Type, ILifetime<object>> services,
        IReadOnlyDictionary<Type, IEnumerable<Type>> interfaces
    )
    {
        _lifetimes = services;
        _interfaces = interfaces;

        _lifetimesToInitialize = new HashSet<IStartupHandler>(
            _lifetimes.Values.OfType<IStartupHandler>()
        );

        InitializeServices();

        foreach (var lifetime in _lifetimes.Values)
        {
            if (lifetime is not IStartupHandler and IDisposable disposable)
            {
                _lifetimesToDispose.AddLast(disposable);
            }
        }
    }

    public object? ResolveOrNull(Type serviceType)
    {
        return GetInitializedLifetime(serviceType)?.GetInstance();
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
        return _lifetimes.ContainsKey(type) || _interfaces.ContainsKey(type);
    }

    public bool Contains<T>()
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

        while (_lifetimesToDispose.FirstOrDefault() is { } disposable)
        {
            _lifetimesToDispose.RemoveFirst();
            disposable.Dispose();
        }
    }

    private ILifetime<object>? GetInitializedLifetime(Type type)
    {
        AssertNotDisposed();

        if (GetLifetime(type) is not { } lifetime)
        {
            return null;
        }

        if (lifetime is IStartupHandler startupHandler)
        {
            InitializeService(startupHandler);
        }

        return lifetime;
    }

    private ILifetime<object>? GetLifetime(Type type)
    {
        AssertNotDisposed();

        if (_lifetimes.TryGetValue(type, out var concreteLifetime))
        {
            return concreteLifetime;
        }

        if (_interfaces.TryGetValue(type, out var implementors))
        {
            return _lifetimes[
                implementors.Last(t =>
                    _lifetimes[t] is not IStartupHandler startupHandler
                    || (_lifetimesToInitialize.Contains(startupHandler) is false)
                )
            ];
        }

        // TODO add IEnumerable<T> support

        return null;
    }

    private void InitializeServices()
    {
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
        service.OnContainerStartup(this);

        if (service is IDisposable disposable)
        {
            _lifetimesToDispose.AddFirst(disposable);
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
