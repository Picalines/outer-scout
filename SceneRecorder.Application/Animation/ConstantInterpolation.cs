namespace SceneRecorder.Application.Animation;

public static class ConstantInterpolation<T>
{
    public static T Interpolate(T left, T right, float progress)
    {
        return progress < 1 ? left : right;
    }
}
