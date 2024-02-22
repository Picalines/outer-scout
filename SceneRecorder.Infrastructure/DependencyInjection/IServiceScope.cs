namespace SceneRecorder.Infrastructure.DependencyInjection;

public interface IServiceScope : IServiceContainer, IDisposable
{
    public IServiceScope StartScope(string identifier);
}
