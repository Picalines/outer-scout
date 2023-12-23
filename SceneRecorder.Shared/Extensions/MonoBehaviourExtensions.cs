using System.Collections;
using UnityEngine;

namespace SceneRecorder.Shared.Extensions;

public static class MonoBehaviourExtensions
{
    public static void InvokeAfterFrame(this MonoBehaviour monoBehaviour, Action action)
    {
        monoBehaviour.StartCoroutine(InvokeAfterFrameCoroutine(action));
    }

    private static IEnumerator InvokeAfterFrameCoroutine(Action action)
    {
        yield return null;

        action();
    }
}
