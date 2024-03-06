using SceneRecorder.Infrastructure.Validation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneRecorder.Infrastructure.DependencyInjection;

using static ServiceContainer;

public static class ServiceContainerExtensions
{
    public static IRegistration<T> InstantiatePerUnityScene<T>(this IRegistration<T> registration)
        where T : class
    {
        return registration.ManageBy(new SceneLifetime<T>());
    }

    public static IRegistration<T> AsGlobalComponent<T>(this IRegistration<T> registration)
        where T : Component
    {
        return registration
            .InstantiatePerUnityScene()
            .InstantiateBy(
                () => new GameObject($"{nameof(SceneRecorder)}.{typeof(T).Name}").AddComponent<T>()
            );
    }

    private sealed class SceneLifetime<T> : ILifetime<T>, IStartupHandler, ICleanupHandler
        where T : class
    {
        private IInstantiator<T>? _instantiator;

        private T? _instance;

        private bool _haveLoadedScene = false;

        public T GetInstance()
        {
            if ((_instance, _haveLoadedScene) is (null, false))
            {
                _instance = _instantiator?.Instantiate();
            }

            _instance.ThrowIfNull(
                _ => new InvalidOperationException($"{typeof(T)} is not created yet")
            );

            return _instance;
        }

        void IStartupHandler.InitializeService(IServiceContainer container)
        {
            _instantiator = container.Resolve<IInstantiator<T>>();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void ICleanupHandler.CleanupService()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneLoaded(Scene _, LoadSceneMode mode)
        {
            if (mode is LoadSceneMode.Single)
            {
                _haveLoadedScene = true;
                _instance = _instantiator?.Instantiate();
            }
        }

        private void OnSceneUnloaded(Scene _)
        {
            (_instance as IDisposable)?.Dispose();
            _instance = null;
        }
    }
}
