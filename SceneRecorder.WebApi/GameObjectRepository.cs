using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.WebApi;

internal sealed class GameObjectRepository
{
    private readonly Dictionary<string, GameObject> _gameObjects = [];

    public GameObject? FindOrNull(string name)
    {
        name.Throw().IfNullOrWhiteSpace().If(name.Contains('/'));

        if (_gameObjects.TryGetValue(name, out var gameObject) is true)
        {
            return gameObject;
        }

        gameObject = GameObject.Find(name);

        _gameObjects.Add(name, gameObject);

        gameObject.AddComponent<DestructionNotifier>().Destroyed += () => _gameObjects.Remove(name);

        return gameObject;
    }
}

internal sealed class DestructionNotifier : MonoBehaviour
{
    public Action? Destroyed;

    private void OnDestroy()
    {
        Destroyed?.Invoke();
        Destroyed = null;
    }
}
