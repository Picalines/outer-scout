using System.Collections.Concurrent;
using UnityEngine;

namespace SceneRecorder.WebApi.Http.Components;

internal sealed class UnityThreadExecutor : MonoBehaviour
{
    private readonly ConcurrentQueue<Action> _tasks = [];

    public static UnityThreadExecutor Create()
    {
        var gameObject = new GameObject($"{nameof(SceneRecorder)}.{nameof(UnityThreadExecutor)}");

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
