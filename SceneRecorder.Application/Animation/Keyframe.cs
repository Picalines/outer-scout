namespace SceneRecorder.Application.Animation;

public delegate T KeyframeInterpolation<T>(Keyframe<T> left, Keyframe<T> right, float progress);

public delegate T ValueInterpolation<T>(T left, T right, float progress);

public readonly record struct Keyframe<T>(
    int Frame,
    T Value,
    KeyframeInterpolation<T> Interpolation
)
{
    public Keyframe(int frame, T value, ValueInterpolation<T> valueInterpolation)
        : this(frame, value, (lk, rk, p) => valueInterpolation(lk.Value, rk.Value, p)) { }

    public Keyframe(int frame, T value)
        : this(frame, value, ConstantInterpolation<T>.Interpolate) { }
}
