using SceneRecorder.Infrastructure.DependencyInjection;
using UnityEngine;

namespace SceneRecorder.WebApi.Components;

internal sealed class SceneResource<T> : InitializedBehaviour<T>, IDisposable
{
    private T _value;

    private bool _destroyGameObject = false;

    private bool _destroyed = false;

    private SceneResource()
        : base(out T value)
    {
        _value = value;
    }

    public static SceneResource<T> CreateGlobal(T value)
    {
        var gameObject = new GameObject($"{nameof(SceneRecorder)}.{nameof(SceneResource<T>)}");

        var resource = gameObject.AddComponent<SceneResource<T>, T>(value);

        resource._destroyGameObject = true;

        return resource;
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
        _destroyed = true;

        (_value as IDisposable)?.Dispose();

        _value = default!;
    }
}
