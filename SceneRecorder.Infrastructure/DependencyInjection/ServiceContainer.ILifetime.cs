namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    public interface ILifetime
    {
        public void SaveInstance(object instance);

        public object GetInstance();

        public void DisposeInstance();
    }

    private sealed class ExternalLifetime
    {
        
    }
}
