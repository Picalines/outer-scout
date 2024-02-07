using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.Infrastructure.DependencyInjection;

public abstract class InitializedBehaviour<TArgs> : MonoBehaviour
{
    private TArgs _args;

    private InitializedBehaviour()
    {
        _args = InitArguments<TArgs>.LastInstance.Value;
    }

    protected InitializedBehaviour(out TArgs args)
        : this()
    {
        args = _args;

        _args = default!;
    }
}

public static partial class InitializedBehaviourExtensions
{
    public static T AddComponent<T, TArgs>(this GameObject gameObject, TArgs args)
        where T : InitializedBehaviour<TArgs>
    {
        using (new InitArguments<TArgs>(args))
        {
            return gameObject.AddComponent<T>();
        }
    }
}

file struct InitArguments<TArgs> : IDisposable
{
    public TArgs Value { get; }

    private bool _disposed = false;

    private static readonly Stack<InitArguments<TArgs>> _stack = new();

    public InitArguments(TArgs args)
    {
        _stack.Push(this);

        Value = args;
    }

    public static InitArguments<TArgs> LastInstance
    {
        get
        {
            _stack.Throw().If(_stack.Count is 0);
            return _stack.Peek();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _stack.Pop();
    }
}
