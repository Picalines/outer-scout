namespace SceneRecorder.Recording.Animation.ValueApplication;

public sealed class LambdaValueApplier<T>(Action<T> apply) : IValueApplier<T>
{
    public void Apply(T value)
    {
        apply(value);
    }
}
