using OuterScout.Infrastructure.DependencyInjection;

namespace OuterScout.WebApi;

internal interface IServiceConfiguration
{
    public void RegisterServices(ServiceContainer.Builder services);
}
