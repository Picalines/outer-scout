using SceneRecorder.Recording.Animation;
using SceneRecorder.Recording.Domain;

namespace SceneRecorder.Recording.Recorders;

public sealed partial class SceneRecorder
{
    public sealed class Builder
    {
        private IntRange _frameRange = new IntRange(1, 100);

        private readonly HashSet<IAnimator> _animators = [];

        private readonly HashSet<IRecorder> _recorders = [];

        private readonly List<ReversableAction> _scenePatches = [];

        public SceneRecorder StartRecording()
        {
            return new(
                _frameRange,
                _animators.ToArray(),
                _recorders.ToArray(),
                _scenePatches.ToArray()
            );
        }

        public Builder WithFrameRange(IntRange frameRange)
        {
            _frameRange = frameRange;
            return this;
        }

        public Builder WithAnimator(IAnimator animator)
        {
            _animators.Add(animator);
            return this;
        }

        public Builder WithRecorder(IRecorder recorder)
        {
            _recorders.Add(recorder);
            return this;
        }

        public Builder WithScenePatch(ReversableAction reversableAction)
        {
            _scenePatches.Add(reversableAction);
            return this;
        }
    }
}
