using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.Infrastructure.DependencyInjection;

file struct InitArguments<T1, T2> : IDisposable
{
    public T1 Argument1 { get; }

    public T2 Argument2 { get; }

    private bool _disposed = false;

    private static readonly Stack<InitArguments<T1, T2>> _stack = new();

    public InitArguments(T1 arg1, T2 arg2)
    {
        _stack.Push(this);

        Argument1 = arg1;
        Argument2 = arg2;
    }

    public static InitArguments<T1, T2> LastInstance
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

public abstract class InitializedBehaviour<T1, T2> : MonoBehaviour
{
    private T1 _arg1;

    private T2 _arg2;

    private InitializedBehaviour()
    {
        var args = InitArguments<T1, T2>.LastInstance;
        _arg1 = args.Argument1;
        _arg2 = args.Argument2;
    }

    protected InitializedBehaviour(out T1 arg1, out T2 arg2)
        : this()
    {
        arg1 = _arg1;
        arg2 = _arg2;

        _arg1 = default!;
        _arg2 = default!;
    }
}

public static partial class InitializedBehaviourExtensions
{
    public static T AddComponent<T, T1, T2>(this GameObject gameObject, T1 arg1, T2 arg2)
        where T : InitializedBehaviour<T1, T2>
    {
        using var _ = new InitArguments<T1, T2>(arg1, arg2);

        return gameObject.AddComponent<T>();
    }
}
