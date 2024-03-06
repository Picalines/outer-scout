using System.Collections;
using OuterScout.Application.Animation;
using OuterScout.Domain;
using OuterScout.Infrastructure.Components;
using OuterScout.Infrastructure.Extensions;
using UnityEngine;

namespace OuterScout.Application.Recording;

public sealed partial class SceneRecorder
{
    public bool IsRecording { get; private set; } = true;

    public int CurrentFrame { get; private set; }

    private readonly IntRange _frameRange;
    private readonly ComposedAnimator _animators;
    private readonly ComposedRecorder _recorders;
    private readonly ReversableAction[] _scenePatches;

    private static readonly WaitForEndOfFrame _waitForEndOfFrame = new();

    private SceneRecorder(
        IntRange frameRange,
        IAnimator[] animators,
        IRecorder[] recorders,
        ReversableAction[] scenePatches
    )
    {
        _frameRange = frameRange;
        _animators = new ComposedAnimator(animators);
        _recorders = new ComposedRecorder(recorders);
        _scenePatches = scenePatches;

        GlobalCoroutine.Start(RecordScene());
    }

    public int FramesRecorded
    {
        get => CurrentFrame - _frameRange.Start;
    }

    private IEnumerator RecordScene()
    {
        CurrentFrame = _frameRange.Start;

        _scenePatches.ForEach(patch => patch.Perform());

        yield return null;

        foreach (var frame in _frameRange)
        {
            CurrentFrame = frame;

            yield return null;

            _animators.ApplyFrame(frame);

            yield return _waitForEndOfFrame;

            _recorders.Capture();
        }

        _scenePatches.Reverse().ForEach(patch => patch.Reverse());

        _recorders.Dispose();

        IsRecording = false;
    }
}
