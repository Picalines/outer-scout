using UnityEngine;

namespace OuterScout.Infrastructure.Extensions;

public static class GameObjectExtensions
{
    public static TComponent? GetComponentOrNull<TComponent>(this GameObject gameObject)
        where TComponent : Component
    {
        return gameObject.GetComponent<TComponent>().OrNull();
    }

    public static TComponent? GetComponentOrNull<TComponent>(this Component component)
        where TComponent : Component
    {
        return component.GetComponent<TComponent>().OrNull();
    }

    public static bool HasComponent<TComponent>(this Component component)
    {
        return component.GetComponent<TComponent>() != null;
    }

    public static bool HasComponent<TComponent>(this GameObject gameObject)
    {
        return gameObject.GetComponent<TComponent>() != null;
    }

    public static TComponent GetOrAddComponent<TComponent>(this GameObject gameObject)
        where TComponent : Component
    {
        if (gameObject.TryGetComponent<TComponent>(out var component) is false)
        {
            component = gameObject.AddComponent<TComponent>();
        }

        return component;
    }

    public static TComponent GetOrAddComponent<TComponent>(this Component component)
        where TComponent : Component
    {
        return component.gameObject.GetOrAddComponent<TComponent>();
    }
}
