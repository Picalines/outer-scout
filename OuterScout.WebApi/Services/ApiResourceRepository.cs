using OuterScout.Shared.DependencyInjection;
using OuterScout.Shared.Extensions;
using UnityEngine;

namespace OuterScout.WebApi.Services;

internal interface IApiResourceContainer
{
    public bool AddResource<T>(string name, T value);

    public T? GetResource<T>(string name);

    public T? GetResource<T>();

    public T GetRequiredResource<T>(string name);

    public T GetRequiredResource<T>();

    public IEnumerable<T> GetResources<T>();

    public void DisposeResource<T>(string name);

    public void DisposeResource<T>();

    public void DisposeResources<T>();
}

internal sealed class ApiResourceRepository : IDisposable
{
    private HashSet<ResourceContainer> _containers = [];

    private GameObject? _globalContainerHost = null;

    public IApiResourceContainer ContainerOf(GameObject gameObject)
    {
        if (gameObject.GetComponent<ResourceContainer>() is not { } container)
        {
            container = gameObject.AddComponent<ResourceContainer, ApiResourceRepository>(this);

            _containers.Add(container);
        }

        return container;
    }

    public IApiResourceContainer GlobalContainer
    {
        get
        {
            if (_globalContainerHost == null)
            {
                _globalContainerHost = new GameObject(
                    $"{nameof(OuterScout)}.{nameof(ApiResourceRepository)}"
                );
            }

            return ContainerOf(_globalContainerHost);
        }
    }

    public IEnumerable<T> GetResources<T>()
        where T : class
    {
        return _containers.SelectMany(c => c.GetResources<T>());
    }

    public void DisposeResources<T>()
    {
        _containers.ToArray().ForEach(c => c.DisposeResources<T>());
    }

    void IDisposable.Dispose()
    {
        _containers.ToArray().ForEach(c => UnityEngine.Object.Destroy(c));
        _containers.Clear();
    }

    private sealed class ResourceContainer
        : InitializedBehaviour<ApiResourceRepository>,
            IApiResourceContainer
    {
        private readonly ApiResourceRepository _repository;

        private readonly Dictionary<string, HashSet<object?>> _resources = [];

        private ResourceContainer()
            : base(out var repository)
        {
            _repository = repository;
        }

        public bool AddResource<T>(string name, T? value)
        {
            return _resources.GetOrCreate(name).Add(value);
        }

        public IEnumerable<T> GetResources<T>()
        {
            return _resources.Values.SelectMany(s => s.OfType<T>());
        }

        public T? GetResource<T>(string name)
        {
            return _resources.TryGetValue(name, out var s)
                ? s.OfType<T>().FirstOrDefault()
                : default;
        }

        public T? GetResource<T>()
        {
            return GetResources<T>().FirstOrDefault();
        }

        public T GetRequiredResource<T>(string name)
        {
            return GetResource<T>(name)
                ?? throw new NullReferenceException(
                    $"required resource '{name}' ({typeof(T)}) was not found"
                );
        }

        public T GetRequiredResource<T>()
        {
            return GetResource<T>()
                ?? throw new NullReferenceException(
                    $"required resource of type {typeof(T)} was not found"
                );
        }

        public void DisposeResource<T>(string name)
        {
            if (
                _resources.GetValueOrDefault(name) is { } instances
                && instances.OfType<T>().FirstOrDefault() is { } resource
            )
            {
                instances.Remove(resource);
                DisposeResource(resource);
            }
        }

        public void DisposeResource<T>()
        {
            foreach (var instances in _resources.Values)
            {
                if (instances.OfType<T>().FirstOrDefault() is { } resource)
                {
                    instances.Remove(resource);
                    DisposeResource(resource);
                    return;
                }
            }
        }

        public void DisposeResources<T>()
        {
            foreach (var instances in _resources.Values)
            {
                foreach (var resource in instances.ToArray().OfType<T>())
                {
                    instances.Remove(resource);
                    DisposeResource(resource);
                }
            }
        }

        private void DisposeResource<T>(T resource)
        {
            if (resource is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private void OnDestroy()
        {
            _repository._containers.Remove(this);

            _resources
                .Values.SelectMany(instances => instances)
                .ToArray()
                .OfType<IDisposable>()
                .ForEach(resource => resource.Dispose());

            _resources.Clear();
        }
    }
}
