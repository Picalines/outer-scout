using System.Collections.Concurrent;
using UnityEngine;

namespace SceneRecorder.Infrastructure.DependencyInjection;

// NOTE:
// Unity creates MonoBehaviour instance in a separate thread, so it's
// impossible to catch exceptions from AddComponent<T, TArgs>.
// Also constructor might be called multiple times in the Editor,
// but (at least now) i don't think that's a problem.

public abstract class InitializedBehaviour<TArgs> : MonoBehaviour
{
    private static readonly ConcurrentStack<TArgs> _argsStack = new();

    private TArgs _args;

    private InitializedBehaviour()
    {
        if (_argsStack.TryPop(out var args) is false)
        {
            throw new InvalidOperationException(
                $"{nameof(InitializedBehaviour<TArgs>)} created without specialized extension method"
            );
        }

        _args = args;
    }

    protected InitializedBehaviour(out TArgs args)
        : this()
    {
        args = _args;

        _args = default!;
    }

    internal static void PushArgs(TArgs args)
    {
        _argsStack.Push(args);
    }
}

public static partial class InitializedBehaviourExtensions
{
    public static T AddComponent<T, TArgs>(this GameObject gameObject, TArgs args)
        where T : InitializedBehaviour<TArgs>
    {
        InitializedBehaviour<TArgs>.PushArgs(args);

        return gameObject.AddComponent<T>();
    }
}
