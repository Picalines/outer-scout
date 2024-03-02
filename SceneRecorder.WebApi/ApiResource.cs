using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.WebApi;

internal interface IApiResource<out T> : IDisposable
    where T : class
{
    public T Value { get; }

    public string Name { get; }

    public bool IsAccessable { get; }
}

internal static class ApiResource
{
    private static HashSet<ResourceContainer> _containers = [];

    private static GameObject? _sceneStore = null;

    public static IApiResource<T> AddApiResource<T>(
        this GameObject gameObject,
        T value,
        string name
    )
        where T : class
    {
        var container = gameObject.GetOrAddComponent<ResourceContainer>();

        _containers.Add(container);

        return container.AddResource(value, name);
    }

    public static IApiResource<T>? GetApiResource<T>(this GameObject gameObject, string name)
        where T : class
    {
        return gameObject.GetComponentOrNull<ResourceContainer>()?.GetResource<T>(name);
    }

    public static IApiResource<T> AddSceneResource<T>(T value, string name)
        where T : class
    {
        return SceneStore.AddApiResource(value, name);
    }

    public static IApiResource<T>? GetSceneResource<T>(string name)
        where T : class
    {
        return SceneStore.GetApiResource<T>(name);
    }

    public static IEnumerable<IApiResource<T>> OfType<T>()
        where T : class
    {
        return _containers.SelectMany(c => c.OfType<T>());
    }

    private static GameObject SceneStore
    {
        get
        {
            if (_sceneStore == null)
            {
                _sceneStore = new GameObject(
                    $"{nameof(SceneRecorder)}.{nameof(ApiResource)}.{nameof(SceneStore)}"
                );
            }

            return _sceneStore;
        }
    }

    private sealed class ResourceContainer : MonoBehaviour
    {
        private readonly Dictionary<string, IApiResource<object>> _resources = [];

        public IApiResource<T> AddResource<T>(T value, string name)
            where T : class
        {
            name.Throw().If(_resources.ContainsKey(name));

            var resource = new Resource<T>(this, value, name);

            _resources[name] = resource;

            return resource;
        }

        public IEnumerable<IApiResource<T>> OfType<T>()
            where T : class
        {
            return _resources.Values.OfType<IApiResource<T>>();
        }

        public IApiResource<T>? GetResource<T>(string name)
            where T : class
        {
            return _resources.TryGetValue(name, out var resource)
                ? resource as IApiResource<T>
                : null;
        }

        private void OnDestroy()
        {
            ApiResource._containers.Remove(this);
            _resources.Values.ToArray().ForEach(resource => resource.Dispose());
            _resources.Clear();
        }

        private sealed class Resource<T> : IApiResource<T>
            where T : class
        {
            private ResourceContainer? _container;

            private T? _value;

            private readonly string _name;

            private bool _disposed = false;

            public Resource(ResourceContainer container, T value, string name)
            {
                _container = container;
                _value = value;
                _name = name;
            }

            public bool IsAccessable
            {
                get => _disposed is false && _container.OrNull() is { };
            }

            public T Value
            {
                get
                {
                    IsAccessable.Throw().IfFalse();
                    return _value!;
                }
            }

            public string Name
            {
                get => _name;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                _container.OrNull()?._resources.Remove(_name);
                _container = null;

                (_value as IDisposable)?.Dispose();
                _value = null;
            }
        }
    }
}
