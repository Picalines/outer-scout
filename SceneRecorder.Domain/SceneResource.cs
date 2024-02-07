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
                $"{nameof(SceneResource)} '{uniqueId}' already exists",
                nameof(uniqueId)
            );
        }

        var resource = gameObject
            .GetOrAddComponent<ResourceContainer>()
            .AddResource(value, uniqueId);

        Instances.Add(resource);
        if (uniqueId is not null)
        {
            InstancesById.Add(uniqueId, resource);
        }

        return resource;
    }

    public static ISceneResource<T>? GetResource<T>(this GameObject gameObject)
        where T : class
    {
        return gameObject.GetComponent<ResourceContainer>().OrNull()?.GetResource<T>();
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
