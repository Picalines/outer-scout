using SceneRecorder.Infrastructure.DependencyInjection;
using UnityEngine;

namespace SceneRecorder.Domain;

public interface ISceneResource<out T> : IDisposable
    where T : class
{
    public T Value { get; }

    internal void InternalOnly();
}

public sealed class SceneResource<T> : InitializedBehaviour<T>, ISceneResource<T>
    where T : class
{
    private T _value;

    private bool _destroyGameObject = false;

    private bool _destroyed = false;

    private SceneResource()
        : base(out T value)
    {
        _value = value;

        SceneResource.Instances.Add(this);
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
        if (_destroyed)
        {
            return;
        }

        _destroyed = true;
        SceneResource.Instances.Remove(this);

        (_value as IDisposable)?.Dispose();
        _value = default!;
    }

    void ISceneResource<T>.InternalOnly()
    {
        throw new NotImplementedException();
    }
}

public static class SceneResource
{
    internal static HashSet<ISceneResource<object>> Instances { get; } = [];

    public static IEnumerable<ISceneResource<T>> FindInstances<T>()
        where T : class
    {
        return Instances.OfType<ISceneResource<T>>().ToArray();
    }
}
