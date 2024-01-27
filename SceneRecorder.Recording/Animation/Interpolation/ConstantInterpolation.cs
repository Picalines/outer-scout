namespace SceneRecorder.Recording.Animation.Interpolation;

public sealed class ConstantInterpolation<T> : IInterpolation<T>
{
    public static ConstantInterpolation<T> Instance { get; } = new();

    private ConstantInterpolation() { }

    public T Interpolate(T left, T right, float progress)
    {
        return progress < 1 ? left : right;
    }
}
