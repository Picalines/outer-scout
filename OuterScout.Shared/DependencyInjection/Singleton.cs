namespace OuterScout.Shared.DependencyInjection;

public static class Singleton<T>
{
    private static Func<T>? _instanceProvider = null;

    public static T Instance
    {
        get
        {
            if (_instanceProvider is null)
            {
                throw new InvalidOperationException($"{nameof(Singleton<T>)} is not configured");
            }

            return _instanceProvider();
        }
    }

    public static void ProvideInstance(Func<T> instanceProvider)
    {
        if (_instanceProvider is not null)
        {
            throw new InvalidOperationException($"{nameof(Singleton<T>)} is already configured");
        }

        _instanceProvider = instanceProvider;
    }

    public static void AssignInstance(T instance)
    {
        ProvideInstance(() => instance);
    }
}
