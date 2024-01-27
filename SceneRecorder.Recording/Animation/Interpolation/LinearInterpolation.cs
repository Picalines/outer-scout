using UnityEngine;

namespace SceneRecorder.Recording.Animation.Interpolation;

public sealed class LinearInterpolation : IInterpolation<float>
{
    public static LinearInterpolation Instance { get; } = new();

    private LinearInterpolation() { }

    public float Interpolate(float left, float right, float progress)
    {
        return Mathf.Lerp(left, right, progress);
    }
}
