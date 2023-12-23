using UnityEngine;

namespace SceneRecorder.Recording.Animators;

internal sealed class TimeScaleAnimator : Animator<float>
{
    public static TimeScaleAnimator Instance { get; } = new();

    private TimeScaleAnimator()
        : base(1) { }

    protected override void ApplyValue(float value)
    {
        Time.timeScale = value;
    }
}
