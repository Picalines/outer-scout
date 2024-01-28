using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.Infrastructure.DependencyInjection;

file struct InitArguments<T1, T2, T3, T4, T5> : IDisposable
{
    public T1 Argument1 { get; }

    public T2 Argument2 { get; }

    public T3 Argument3 { get; }

    public T4 Argument4 { get; }

    public T5 Argument5 { get; }

    private static readonly Stack<InitArguments<T1, T2, T3, T4, T5>> _stack = new();

    public InitArguments(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        _stack.Push(this);

        Argument1 = arg1;
        Argument2 = arg2;
        Argument3 = arg3;
        Argument4 = arg4;
        Argument5 = arg5;
    }

    public static InitArguments<T1, T2, T3, T4, T5> LastInstance
    {
        get
        {
            _stack.Throw().If(_stack.Count is 0);
            return _stack.Peek();
        }
    }

    public void Dispose()
    {
        _stack.Pop();
    }
}

public abstract class InitializedBehaviour<T1, T2, T3, T4, T5> : MonoBehaviour
{
    private T1 _arg1;

    private T2 _arg2;

    private T3 _arg3;

    private T4 _arg4;

    private T5 _arg5;

    private InitializedBehaviour()
    {
        var args = InitArguments<T1, T2, T3, T4, T5>.LastInstance;
        _arg1 = args.Argument1;
        _arg2 = args.Argument2;
        _arg3 = args.Argument3;
        _arg4 = args.Argument4;
        _arg5 = args.Argument5;
    }

    protected InitializedBehaviour(out T1 arg1, out T2 arg2, out T3 arg3, out T4 arg4, out T5 arg5)
        : this()
    {
        arg1 = _arg1;
        arg2 = _arg2;
        arg3 = _arg3;
        arg4 = _arg4;
        arg5 = _arg5;

        _arg1 = default!;
        _arg2 = default!;
        _arg3 = default!;
        _arg4 = default!;
        _arg5 = default!;
    }
}

public static partial class InitializedBehaviourExtensions
{
    public static T AddComponent<T, T1, T2, T3, T4, T5>(
        this GameObject gameObject,
        T1 arg1,
        T2 arg2,
        T3 arg3,
        T4 arg4,
        T5 arg5
    )
        where T : InitializedBehaviour<T1, T2, T3, T4, T5>
    {
        using var _ = new InitArguments<T1, T2, T3, T4, T5>(arg1, arg2, arg3, arg4, arg5);

        return gameObject.AddComponent<T>();
    }
}