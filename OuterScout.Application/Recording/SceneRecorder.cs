using System.Collections;
using OuterScout.Application.Animation;
using OuterScout.Shared.Collections;
using OuterScout.Shared.Components;
using OuterScout.Shared.DependencyInjection;
using OuterScout.Shared.Extensions;
using OuterScout.Shared.Validation;
using OWML.Common;
using UnityEngine;

namespace OuterScout.Application.Recording;

public sealed partial class SceneRecorder
{
    public bool IsRecording { get; private set; } = true;

    public int CurrentFrame { get; private set; }

    private readonly RecordingParameters _recordingPrameters;
    private readonly ComposedAnimator _animators;
    private readonly IRecorder.IBuilder[] _recorderBuilders;
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
        _recorderBuilders = recorders;
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
        Time.captureFramerate = FrameRate; // NOTE: must be set before start, used by recorders
        _scenePatches.ForEach(patch => patch.PerformIfNotAlready());

        ComposedRecorder? recorders = null;

        try
        {
            recorders = new ComposedRecorder(StartRecordersOrRecover());

            foreach (var frame in FrameRange)
            {
                yield return null;

                _animators.ApplyFrame(CurrentFrame = frame);

                yield return _waitForEndOfFrame;

                recorders.Capture();
            }
        }
        finally
        {
            recorders?.Dispose();

            _scenePatches.Reverse().ForEach(patch => patch.ReverseIfPerformed());

            Time.captureFramerate = 0;
            IsRecording = false;
        }
    }

    private IReadOnlyList<IRecorder> StartRecordersOrRecover()
    {
        var startedRecorders = new List<IRecorder>();

        try
        {
            foreach (var builder in _recorderBuilders)
            {
                startedRecorders.Add(builder.StartRecording());
            }
        }
        catch
        {
            // recorders might do heavy operations, so we dispose them back if one crashes
            startedRecorders.ForEach(recorder => recorder.Dispose());
            IsRecording = false;
            throw;
        }

        return startedRecorders;
    }
}
