using System.Collections;
using OuterScout.Shared.DependencyInjection;
using OuterScout.Shared.Validation;
using OWML.Common;
using UnityEngine;

namespace OuterScout.Shared.Components;

public sealed class GlobalCoroutine : MonoBehaviour
{
    private IEnumerator _coroutine = null!;

    private GlobalCoroutine() { }

    private void Start()
    {
        _coroutine.AssertNotNull();

        StartCoroutine(YieldAndDestory());
    }

    public static void Start(IEnumerator coroutine)
    {
        var gameObject = new GameObject($"{nameof(OuterScout)}.{nameof(GlobalCoroutine)}");

        gameObject.SetActive(false);

        var globalCoroutine = gameObject.AddComponent<GlobalCoroutine>();

        globalCoroutine._coroutine = coroutine;

        gameObject.SetActive(true);
    }

    private IEnumerator YieldAndDestory()
    {
        var shouldYield = true;

        while (shouldYield)
        {
            try
            {
                shouldYield = _coroutine.MoveNext();
            }
            catch (Exception exception)
            {
                var console = Singleton<IModConsole>.Instance;
                console.WriteLine(
                    $"{exception.GetType().FullName} thrown in a {nameof(GlobalCoroutine)}",
                    MessageType.Error
                );
                console.WriteLine(exception.ToString(), MessageType.Error);
                break;
            }

            if (shouldYield)
            {
                yield return _coroutine.Current;
            }
        }

        Destroy(gameObject);
    }
}
