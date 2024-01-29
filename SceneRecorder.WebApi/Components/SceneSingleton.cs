using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.WebApi.Components;

internal sealed class SceneSingleton<T> : MonoBehaviour
{
    private static SceneSingleton<T>? _singletonInstance = null;

    private static Func<T>? _instanceFactory = null;

    private readonly T _instance;

    private SceneSingleton()
    {
        _instanceFactory.ThrowIfNull();

        _instance = _instanceFactory();

        gameObject.name = $"{nameof(SceneRecorder)}.{nameof(SceneSingleton<T>)}";
    }

    private void OnDestroy()
    {
        _singletonInstance = null;
    }

    public static void ProvideFactory(Func<T> instanceFactory)
    {
        if (_instanceFactory is not null)
        {
            throw new InvalidOperationException(
                $"{nameof(SceneSingleton<T>)} is already configured"
            );
        }

        _instanceFactory = instanceFactory;
    }

    public static T Instance
    {
        get
        {
            if (_instanceFactory is null)
            {
                throw new InvalidOperationException(
                    $"{nameof(SceneSingleton<T>)} is not configured"
                );
            }

            _singletonInstance ??= new GameObject().AddComponent<SceneSingleton<T>>();

            return _singletonInstance._instance;
        }
    }
}
