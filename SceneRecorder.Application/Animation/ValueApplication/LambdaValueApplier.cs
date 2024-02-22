namespace SceneRecorder.Application.Animation.ValueApplication;

public sealed class LambdaValueApplier<T>(Action<T> apply) : IValueApplier<T>
{
    public void Apply(T value)
    {
        apply(value);
    }
}

public static partial class ValueApplier
{
    public static LambdaValueApplier<T> Lambda<T>(Action<T> apply) =>
        new LambdaValueApplier<T>(apply);
}
