using System.Collections.Concurrent;
using UnityEngine;

namespace OuterScout.WebApi.Http.Components;

internal sealed class UnityThreadExecutor : MonoBehaviour
{
    private readonly ConcurrentQueue<Action> _tasks = [];

    public static UnityThreadExecutor Create()
    {
        var gameObject = new GameObject($"{nameof(OuterScout)}.{nameof(UnityThreadExecutor)}");

        DontDestroyOnLoad(gameObject);

        return gameObject.AddComponent<UnityThreadExecutor>();
    }

    public void EnqueueTask(Action task)
    {
        _tasks.Enqueue(task);
    }

    private void Update()
    {
        if (_tasks.TryDequeue(out var action))
        {
            action();
        }
    }

    private void OnDestroy()
    {
        _tasks.Clear();

        Destroy(gameObject);
    }
}
