using System.Diagnostics.CodeAnalysis;
using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.WebApi.Components;

internal sealed class SceneSingleton<T> : MonoBehaviour
{
    private static SceneSingleton<T>? _singletonInstance = null;

    private static Func<T>? _instanceFactory = null;

    private T _instance = default!;

    private SceneSingleton()
    {
        _instanceFactory.ThrowIfNull();

        gameObject.name = $"{nameof(SceneRecorder)}.{nameof(SceneSingleton<T>)}";
    }

    private void OnDestroy()
    {
        _singletonInstance = null;
    }

    public static bool IsConfigured => _instanceFactory is not null;

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

            if (_singletonInstance is null)
            {
                var instance = _instanceFactory();

                _singletonInstance = new GameObject().AddComponent<SceneSingleton<T>>();

                _singletonInstance._instance = instance;
            }

            return _singletonInstance._instance;
        }
    }
}
