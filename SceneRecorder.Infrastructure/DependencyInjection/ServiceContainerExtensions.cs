namespace SceneRecorder.Infrastructure.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static void WithSingleton<T>(this ServiceContainer.Builder services, T instance)
        where T : class
    {
        return services.RegisterService(new SingletonService<T>(instance));
    }

    

    public static void WithFactory<T>(
        this ServiceContainer services,
        Func<T> instanceFactory
    )
        where T : class
    {
        return services.RegisterService(new FactoryService<T>(instanceFactory));
    }

    private sealed class SingletonLifetimeManager<T> : ServiceContainer.LifetimeManager<T>
        where T : class
    {
        private 
        public override T GetInstance()
        {
            throw new NotImplementedException();
        }
    }


}
