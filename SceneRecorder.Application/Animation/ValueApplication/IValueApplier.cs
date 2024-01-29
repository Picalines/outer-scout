namespace SceneRecorder.Application.Animation.ValueApplication;

public interface IValueApplier<T>
{
    public void Apply(T value);
}