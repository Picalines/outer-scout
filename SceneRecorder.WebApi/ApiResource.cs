using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.WebApi;

internal interface IApiResource<out T> : IDisposable
    where T : class
{
    public T Value { get; }

    public bool IsAccessable { get; }
}

internal static class ApiResource
{
    private static HashSet<IApiResource<object>> _resouces { get; } = [];

    private static Dictionary<string, IApiResource<object>> _resourcesWithId = [];

    public static IApiResource<T> AddApiResource<T>(
        this GameObject gameObject,
        T value,
        string? uniqueId = null
    )
        where T : class
    {
        uniqueId.Throw().If(uniqueId is not null && _resourcesWithId.ContainsKey(uniqueId));

        var resource = gameObject
            .GetOrAddComponent<ResourceContainer>()
            .AddResource(value, uniqueId);

        _resouces.Add(resource);
        if (uniqueId is not null)
        {
            _resourcesWithId.Add(uniqueId, resource);
        }

        return resource;
    }

    public static IApiResource<T>? GetApiResource<T>(this GameObject gameObject)
        where T : class
    {
        return gameObject.GetComponent<ResourceContainer>().OrNull()?.GetResource<T>();
    }

    public static IEnumerable<IApiResource<T>> Find<T>()
        where T : class
    {
        return _resouces.OfType<IApiResource<T>>().ToArray();
    }

    public static IApiResource<T>? Find<T>(string id)
        where T : class
    {
        return _resourcesWithId.TryGetValue(id, out var resource)
            ? resource as IApiResource<T>
            : null;
    }

    private sealed class ResourceContainer : MonoBehaviour
    {
        private readonly HashSet<IApiResource<object>> _resources = [];

        public IApiResource<T> AddResource<T>(T value, string? uniqueId)
            where T : class
        {
            var resource = new Resource<T>(this, value, uniqueId);

            _resources.Add(resource);

            return resource;
        }

        public IApiResource<T>? GetResource<T>()
            where T : class
        {
            return _resources.OfType<IApiResource<T>>().FirstOrDefault();
        }

        private void OnDestroy()
        {
            _resources.ToArray().ForEach(resource => resource.Dispose());
            _resources.Clear();
        }

        private sealed class Resource<T> : IApiResource<T>
            where T : class
        {
            private ResourceContainer? _container;

            private T? _value;

            private readonly string? _id;

            private bool _disposed = false;

            public Resource(ResourceContainer container, T value, string? uniqueId)
            {
                _container = container;
                _value = value;
                _id = uniqueId;
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

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                _container.OrNull()?._resources.Remove(this);
                _container = null;

                ApiResource._resouces.Remove(this);
                if (_id is not null)
                {
                    ApiResource._resourcesWithId.Remove(_id);
                }

                (_value as IDisposable)?.Dispose();
                _value = null;
            }
        }
    }
}
