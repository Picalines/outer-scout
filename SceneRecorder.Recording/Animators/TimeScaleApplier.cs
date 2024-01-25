using UnityEngine;

namespace SceneRecorder.Recording.Animators;

public sealed class TimeScaleApplier : IValueApplier<float>
{
    public static TimeScaleApplier Instance { get; } = new();

    private TimeScaleApplier() { }

    public void Apply(float value)
    {
        Time.timeScale = value;
    }
}
