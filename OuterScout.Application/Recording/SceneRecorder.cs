using System.Collections;
using OuterScout.Application.Animation;
using OuterScout.Domain;
using OuterScout.Infrastructure.Components;
using OuterScout.Infrastructure.Extensions;
using OuterScout.Infrastructure.Validation;
using UnityEngine;

namespace OuterScout.Application.Recording;

public sealed partial class SceneRecorder
{
    public bool IsRecording { get; private set; } = true;

    public int CurrentFrame { get; private set; }

    private readonly RecordingParameters _recordingPrameters;
    private readonly ComposedAnimator _animators;
    private readonly IRecorder.IBuilder[] _recorders;
    private readonly ReversableAction[] _scenePatches;

    private static readonly WaitForEndOfFrame _waitForEndOfFrame = new();

    private SceneRecorder(
        RecordingParameters recordingParameters,
        PropertyAnimator[] animators,
        IRecorder.IBuilder[] recorders,
        ReversableAction[] scenePatches
    )
    {
        recordingParameters.FrameRate.Throw().IfLessThan(1);

        _recordingPrameters = recordingParameters;
        _animators = new ComposedAnimator(animators);
        _recorders = recorders;
        _scenePatches = scenePatches;

        GlobalCoroutine.Start(RecordScene());
    }

    public IntRange FrameRange
    {
        get => _recordingPrameters.FrameRange;
    }

    public int FrameRate
    {
        get => _recordingPrameters.FrameRate;
    }

    public int FramesRecorded
    {
        get => CurrentFrame - FrameRange.Start;
    }

    private IEnumerator RecordScene()
    {
        CurrentFrame = FrameRange.Start;

        _scenePatches.ForEach(patch => patch.Perform());
        Time.captureFramerate = FrameRate;

        var recorders = new ComposedRecorder(_recorders.Select(r => r.StartRecording()).ToArray());

        yield return null;

        foreach (var frame in FrameRange)
        {
            _animators.ApplyFrame(CurrentFrame = frame);

            yield return _waitForEndOfFrame;

            recorders.Capture();
        }

        Time.captureFramerate = 0;
        _scenePatches.Reverse().ForEach(patch => patch.Reverse());

        recorders.Dispose();

        IsRecording = false;
    }
}
