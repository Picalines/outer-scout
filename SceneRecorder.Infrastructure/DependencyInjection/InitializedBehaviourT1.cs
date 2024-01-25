using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.Infrastructure.DependencyInjection;

file struct InitArguments<T1> : IDisposable
{
    public T1 Argument1 { get; }

    private bool _disposed = false;

    private static readonly Stack<InitArguments<T1>> _stack = new();

    public InitArguments(T1 arg1)
    {
        _stack.Push(this);

        Argument1 = arg1;
    }

    public static InitArguments<T1> LastInstance
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

public abstract class InitializedBehaviour<T1> : MonoBehaviour
{
    private T1 _arg1;

    private InitializedBehaviour()
    {
        var args = InitArguments<T1>.LastInstance;
        _arg1 = args.Argument1;
    }

    protected InitializedBehaviour(out T1 arg1)
        : this()
    {
        arg1 = _arg1;

        _arg1 = default!;
    }
}

public static partial class InitializedBehaviourExtensions
{
    public static T AddComponent<T, T1>(this GameObject gameObject, T1 arg1)
        where T : InitializedBehaviour<T1>
    {
        using var _ = new InitArguments<T1>(arg1);

        return gameObject.AddComponent<T>();
    }
}

