using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.Domain;

internal sealed class ResourceContainer : MonoBehaviour
{
    private readonly HashSet<ISceneResource<object>> _resources = [];

    public ISceneResource<T> AddResource<T>(T value, string? uniqueId)
        where T : class
    {
        var resource = new SceneResource<T>(this, value, uniqueId);

        _resources.Add(resource);

        return resource;
    }

    public ISceneResource<T>? GetResource<T>()
        where T : class
    {
        return _resources.OfType<ISceneResource<T>>().FirstOrDefault();
    }

    private void OnDestroy()
    {
        _resources.ToArray().ForEach(resource => resource.Dispose());
        _resources.Clear();
    }

    private sealed class SceneResource<T> : ISceneResource<T>
        where T : class
    {
        private ResourceContainer? _container;

        private T? _value;

        private readonly string? _id;

        private bool _disposed = false;

        public SceneResource(ResourceContainer container, T value, string? uniqueId)
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

            SceneResource.Instances.Remove(this);
            if (_id is not null)
            {
                SceneResource.InstancesById.Remove(_id);
            }

            (_value as IDisposable)?.Dispose();
            _value = null;
        }

        public void InternalOnly() { }
    }
}
