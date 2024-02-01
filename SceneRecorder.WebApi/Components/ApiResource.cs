using SceneRecorder.Infrastructure.DependencyInjection;

namespace SceneRecorder.WebApi.Components;

internal sealed class ApiResource<T> : InitializedBehaviour<T>
{
    private T _value;

    private bool _destroyed = false;

    private ApiResource()
        : base(out T value)
    {
        _value = value;
    }

    public T Value
    {
        get
        {
            if (_destroyed)
            {
                throw new InvalidOperationException($"{nameof(ApiResource<T>)} is destroyed");
            }

            return _value;
        }
    }

    public bool IsAccessable
    {
        get => _destroyed is false;
    }

    private void OnDestroy()
    {
        _destroyed = true;

        (_value as IDisposable)?.Dispose();

        _value = default!;
    }
}
