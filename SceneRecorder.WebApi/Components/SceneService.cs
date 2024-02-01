using SceneRecorder.Infrastructure.DependencyInjection;
using UnityEngine;

namespace SceneRecorder.WebApi.Components;

internal sealed class SceneService<T>(Func<T> instanceFactory) : IService<T>
{
    private ApiResource<T>? _resource = null;

    public T GetInstance()
    {
        if (_resource is not { IsAccessable: true })
        {
            var instance = instanceFactory();

            var gameObject = new GameObject($"{nameof(SceneRecorder)}.{nameof(SceneService<T>)}");

            _resource = gameObject.AddComponent<ApiResource<T>, T>(instance);
        }

        return _resource.Value;
    }
}
