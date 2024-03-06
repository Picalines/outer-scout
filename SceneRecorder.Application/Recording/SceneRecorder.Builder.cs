using OuterScout.Application.Animation;
using OuterScout.Domain;
using OuterScout.Infrastructure.Validation;
using UnityEngine;

namespace OuterScout.Application.Recording;

public sealed partial class SceneRecorder
{
    public sealed class Builder
    {
        private IntRange _frameRange = IntRange.FromValues(1, 100);

        private int _captureFrameRate = 30;

        private readonly HashSet<IAnimator> _animators = [];

        private readonly List<IRecorder.IBuilder> _recorderBuilders = [];

        private readonly List<ReversableAction> _scenePatches = [];

        public Builder()
        {
            WithCaptureFrameRatePatch();
        }

        public IntRange FrameRange
        {
            get => _frameRange;
        }

        public int CaptureFrameRate
        {
            get => _captureFrameRate;
        }

        public SceneRecorder StartRecording()
        {
            return new(
                _frameRange,
                _animators.ToArray(),
                _recorderBuilders.Select(builder => builder.StartRecording()).ToArray(),
                _scenePatches.ToArray()
            );
        }

        public Builder WithFrameRange(IntRange frameRange)
        {
            _frameRange = frameRange;
            return this;
        }

        public Builder WithCaptureFrameRate(int frameRate)
        {
            frameRate.Throw().IfLessThan(1);
            _captureFrameRate = frameRate;
            return this;
        }

        public Builder WithAnimator(IAnimator animator)
        {
            _animators.Add(animator);
            return this;
        }

        public Builder WithRecorder(IRecorder.IBuilder recorderBuilder)
        {
            _recorderBuilders.Add(recorderBuilder);
            return this;
        }

        public Builder WithScenePatch(ReversableAction reversableAction)
        {
            _scenePatches.Add(reversableAction);
            return this;
        }

        public Builder WithScenePatch(Action apply, Action restore)
        {
            return WithScenePatch(new(apply, restore));
        }

        private Builder WithCaptureFrameRatePatch()
        {
            int oldCaptureFrameRate = 0;

            return WithScenePatch(
                () =>
                {
                    oldCaptureFrameRate = Time.captureFramerate;
                    Time.captureFramerate = _captureFrameRate;
                },
                () => Time.captureFramerate = oldCaptureFrameRate
            );
        }
    }
}
