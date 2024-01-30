namespace SceneRecorder.Infrastructure.DependencyInjection;

public interface IService<out T>
{
    public T GetInstance();
}
