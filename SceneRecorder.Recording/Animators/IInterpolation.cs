namespace SceneRecorder.Recording.Animators;

public interface IInterpolation<T>
{
    public T Interpolate(T left, T right, float progress);
}
