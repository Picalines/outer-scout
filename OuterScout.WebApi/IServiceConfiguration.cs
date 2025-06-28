using OuterScout.Shared.DependencyInjection;

namespace OuterScout.WebApi;

internal interface IServiceConfiguration
{
    public void RegisterServices(ServiceContainer.Builder services);
}
