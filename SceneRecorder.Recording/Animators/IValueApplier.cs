namespace SceneRecorder.Recording.Animators;

public interface IValueApplier<T>
{
    public void Apply(T value);
}
