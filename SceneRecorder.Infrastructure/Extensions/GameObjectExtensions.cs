using UnityEngine;

namespace SceneRecorder.Infrastructure.Extensions;

public static class GameObjectExtensions
{
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
