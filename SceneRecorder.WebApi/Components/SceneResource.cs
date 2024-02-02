using SceneRecorder.Infrastructure.DependencyInjection;
using UnityEngine;

namespace SceneRecorder.WebApi.Components;

internal sealed class SceneResource<T> : InitializedBehaviour<T>, IDisposable
{
    private T _value;

    private bool _destroyGameObject = false;

    private bool _destroyed = false;

    private static HashSet<SceneResource<T>> _instances = [];

    private SceneResource()
        : base(out T value)
    {
        _value = value;

        _instances.Add(this);
    }

    public static SceneResource<T> CreateGlobal(T value)
    {
        var gameObject = new GameObject($"{nameof(SceneRecorder)}.{nameof(SceneResource<T>)}");

        var resource = gameObject.AddComponent<SceneResource<T>, T>(value);

        resource._destroyGameObject = true;

        return resource;
    }

    public static IEnumerable<SceneResource<T>> Instances
    {
        get => _instances;
    }

    public T Value
    {
        get
        {
            if (_destroyed)
            {
                throw new InvalidOperationException($"{nameof(SceneResource<T>)} is destroyed");
            }

            return _value;
        }
    }

    public bool IsAccessable
    {
        get => _destroyed is false;
    }

    public void Dispose()
    {
        if (_destroyed)
        {
            return;
        }

        Destroy(_destroyGameObject ? gameObject : this);
    }

    private void OnDestroy()
    {
        if (_destroyed)
        {
            return;
        }

        _instances.Remove(this);
        _destroyed = true;

        (_value as IDisposable)?.Dispose();
        _value = default!;
    }
}
