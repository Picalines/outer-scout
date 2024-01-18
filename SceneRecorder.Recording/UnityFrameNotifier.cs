using System.Collections;
using UnityEngine;

namespace SceneRecorder.Recording;

internal sealed class UnityFrameNotifier : MonoBehaviour
{
    public event Action? FrameStarted;

    public event Action? FrameEnded;

    private static readonly WaitForEndOfFrame _waitForEndOfFrame = new();

    private void OnEnable()
    {
        StartCoroutine(Notify());
    }

    private IEnumerator Notify()
    {
        while (enabled)
        {
            yield return null;

            FrameStarted?.Invoke();

            yield return _waitForEndOfFrame;

            FrameEnded?.Invoke();
        }
    }
}
