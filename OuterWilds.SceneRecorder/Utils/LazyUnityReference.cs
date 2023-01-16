using System;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Utils;

internal sealed class LazyUnityReference<TObject> where TObject : UnityEngine.Object
{
    private TObject? _Object = null;

    private readonly Func<TObject> _GetObject;

    public LazyUnityReference(Func<TObject> getObject)
    {
        _GetObject = getObject;
    }

    public TObject Object
    {
        get
        {
            if (_Object == null)
            {
                _Object = _GetObject();
            }

            return _Object;
        }
    }

    public bool ObjectExists
    {
        get => Object != null;
    }
}

internal static class LazyUnityReference
{
    public static LazyUnityReference<GameObject> FromFind(string name)
    {
        return new LazyUnityReference<GameObject>(() => GameObject.Find(name));
    }

    public static LazyUnityReference<TComponent> FromFind<TComponent>(string name) where TComponent : Component
    {
        return new LazyUnityReference<TComponent>(() => GameObject.Find(name)?.GetComponent<TComponent>()!);
    }
}
