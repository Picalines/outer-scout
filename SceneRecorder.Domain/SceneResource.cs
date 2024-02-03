using SceneRecorder.Infrastructure.DependencyInjection;
using SceneRecorder.Infrastructure.Extensions;
using UnityEngine;

namespace SceneRecorder.Domain;

public interface ISceneResource<out T> : IDisposable
    where T : class
{
    public T Value { get; }

    public bool IsAccessable { get; }

    internal void InternalOnly();
}

public static class SceneResource
{
    internal static HashSet<ISceneResource<object>> Instances { get; } = [];

    internal static Dictionary<string, ISceneResource<object>> InstancesById = [];

    public static ISceneResource<T> AddResource<T>(
        this GameObject gameObject,
        T value,
        string? uniqueId = null
    )
        where T : class
    {
        if (uniqueId is not null && InstancesById.ContainsKey(uniqueId))
        {
            throw new ArgumentException(
                $"{nameof(SceneResource<T>)} {uniqueId} already exists",
                nameof(uniqueId)
            );
        }

        return gameObject.AddComponent<SceneResource<T>, SceneResource<T>.Parameters>(
            new() { Value = value, Id = uniqueId }
        );
    }

    public static ISceneResource<T>? GetResource<T>(this GameObject gameObject)
        where T : class
    {
        return gameObject.GetComponent<SceneResource<T>>().OrNull();
    }

    public static IEnumerable<ISceneResource<T>> Find<T>()
        where T : class
    {
        return Instances.OfType<ISceneResource<T>>().ToArray();
    }

    public static ISceneResource<T>? Find<T>(string id)
        where T : class
    {
        return InstancesById.TryGetValue(id, out var resource)
            ? resource as ISceneResource<T>
            : null;
    }
}

internal sealed class SceneResource<T>
    : InitializedBehaviour<SceneResource<T>.Parameters>,
        ISceneResource<T>
    where T : class
{
    public struct Parameters
    {
        public required T Value { get; init; }

        public required string? Id { get; init; }
    }

    private T _value;

    private readonly string? _id;

    private bool _destroyed = false;

    private SceneResource()
        : base(out var parameters)
    {
        _value = parameters.Value;
        _id = parameters.Id;

        SceneResource.Instances.Add(this);

        if (_id is not null)
        {
            SceneResource.InstancesById[_id] = this;
        }
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

        Destroy(this);
    }

    private void OnDestroy()
    {
        if (_destroyed)
        {
            return;
        }

        _destroyed = true;
        SceneResource.Instances.Remove(this);
        if (_id is not null)
        {
            SceneResource.InstancesById.Remove(_id);
        }

        (_value as IDisposable)?.Dispose();
        _value = default!;
    }

    void ISceneResource<T>.InternalOnly()
    {
        throw new NotImplementedException();
    }
}
