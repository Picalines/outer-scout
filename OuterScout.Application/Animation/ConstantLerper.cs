namespace OuterScout.Application.Animation;

public static class ConstantLerper<T>
{
    public static T Lerp(T left, T right, float progress)
    {
        return progress < 1 ? left : right;
    }
}
