namespace SceneRecorder.Application.Animation.Interpolation;

public interface IInterpolation<T>
{
    public T Interpolate(T left, T right, float progress);
}
