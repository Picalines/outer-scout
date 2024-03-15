using OuterScout.Infrastructure.Extensions;
using OuterScout.Infrastructure.Validation;
using UnityEngine;

namespace OuterScout.WebApi.Services;

internal sealed class GameObjectRepository
{
    private readonly ApiResourceRepository _apiResources;

    public GameObjectRepository(ApiResourceRepository apiResources)
    {
        _apiResources = apiResources;
    }

    public bool Contains(string name)
    {
        return _apiResources.GlobalContainer.GetResource<GameObject>(name) is not null;
    }

    public void AddOwned(string name, GameObject gameObject)
    {
        gameObject.OrNull().ThrowIfNull();

        name.Throw()
            .IfNullOrWhiteSpace()
            .If(name.Contains('/'))
            .If(FindOrNull(name) is not null)
            .If(Contains(name));

        bool added = AddExternal(name, gameObject);

        if (added)
        {
            _apiResources.GlobalContainer.AddResource(
                name,
                gameObject.GetOrAddComponent<ApiOwnedGameObject>()
            );
        }
    }

    public GameObject? FindOrNull(string name)
    {
        name.Throw().IfNullOrWhiteSpace().If(name.Contains('/'));

        if (_apiResources.GlobalContainer.GetResource<GameObject>(name) is { } gameObjectInRepo)
        {
            return gameObjectInRepo.OrNull();
        }

        if (GameObject.Find(name).OrNull() is { } gameObject)
        {
            AddExternal(name, gameObject);

            return gameObject;
        }

        return null;
    }

    private bool AddExternal(string name, GameObject gameObject)
    {
        bool added = _apiResources.GlobalContainer.AddResource(name, gameObject);

        if (added)
        {
            gameObject.GetOrAddComponent<DestructionNotifier>().Destroyed += () =>
            {
                _apiResources.GlobalContainer.DisposeResource<GameObject>(name);
            };
        }

        return added;
    }

    private sealed class DestructionNotifier : MonoBehaviour
    {
        public Action? Destroyed;

        private void OnDestroy()
        {
            Destroyed?.Invoke();
            Destroyed = null;
        }
    }
}

internal sealed class ApiOwnedGameObject : MonoBehaviour, IDisposable
{
    private GameObject? _gameObject;

    private void Start()
    {
        _gameObject = gameObject;
    }

    void IDisposable.Dispose()
    {
        if (_gameObject != null)
        {
            UnityEngine.Object.DestroyImmediate(_gameObject);
            _gameObject = null;
        }
    }
}
