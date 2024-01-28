using System.Collections;
using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.Application.Components;

internal sealed class GlobalCoroutine : MonoBehaviour
{
    private IEnumerator _coroutine = null!;

    private GlobalCoroutine() { }

    private void Start()
    {
        _coroutine.ThrowIfNull();

        StartCoroutine(YieldAndDestory());
    }

    public static void Start(IEnumerator coroutine)
    {
        var gameObject = new GameObject($"{nameof(GlobalCoroutine)}");

        gameObject.SetActive(false);

        var globalCoroutine = gameObject.AddComponent<GlobalCoroutine>();

        globalCoroutine._coroutine = coroutine;

        gameObject.SetActive(true);
    }

    private IEnumerator YieldAndDestory()
    {
        while (_coroutine.MoveNext() is true)
        {
            yield return _coroutine.Current;
        }

        Destroy(gameObject);
    }
}
