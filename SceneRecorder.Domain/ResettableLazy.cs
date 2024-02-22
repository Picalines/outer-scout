namespace SceneRecorder.Domain;

public sealed class ResettableLazy<T>
{
    private readonly Func<T> _factory;

    private bool _isValueCreated = false;

    private T _value = default!;

    public ResettableLazy(Func<T> factory)
    {
        _factory = factory;
    }

    public bool IsValueCreated
    {
        get => _isValueCreated;
    }

    public T Value
    {
        get
        {
            if (IsValueCreated is false)
            {
                _value = _factory();
                _isValueCreated = true;
            }

            return _value;
        }
    }

    public void Reset()
    {
        _isValueCreated = false;
        _value = default!;
    }
}

public static class ResettableLazy
{
    public static ResettableLazy<T> Of<T>(Func<T> factory)
    {
        return new ResettableLazy<T>(factory);
    }
}
