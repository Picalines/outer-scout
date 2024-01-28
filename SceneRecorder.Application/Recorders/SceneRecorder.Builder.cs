using SceneRecorder.Domain;
using SceneRecorder.Recording.Animation;

namespace SceneRecorder.Recording.Recorders;

public sealed partial class SceneRecorder
{
    public sealed class Builder
    {
        private IntRange _frameRange = new IntRange(1, 100);

        private readonly HashSet<IAnimator> _animators = [];

        private readonly List<Func<IRecorder>> _recorderFactories = [];

        private readonly List<ReversableAction> _scenePatches = [];

        public SceneRecorder StartRecording()
        {
            return new(
                _frameRange,
                _animators.ToArray(),
                _recorderFactories.Select(f => f()).ToArray(),
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

        public Builder WithRecorder(Func<IRecorder> recorderFactory)
        {
            _recorderFactories.Add(recorderFactory);
            return this;
        }

        public Builder WithScenePatch(ReversableAction reversableAction)
        {
            _scenePatches.Add(reversableAction);
            return this;
        }
    }
}
