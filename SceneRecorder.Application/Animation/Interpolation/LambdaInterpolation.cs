namespace SceneRecorder.Application.Animation.Interpolation;

public sealed class LambdaInterpolation<T>(Func<T, T, float, T> interpolate) : IInterpolation<T>
{
    public T Interpolate(T left, T right, float progress)
    {
        return interpolate(left, right, progress);
    }
}
