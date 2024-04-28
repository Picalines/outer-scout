using OuterScout.Application.Animation;
using OuterScout.Application.Extensions;
using OuterScout.Domain;

namespace OuterScout.Application.Recording;

public sealed partial class SceneRecorder
{
    public sealed class Builder
    {
        private readonly OrderedSet<PropertyAnimator> _animators = [];

        private readonly HashSet<IRecorder.IBuilder> _recorderBuilders = [];

        private readonly List<ReversableAction> _scenePatches = [];

        public SceneRecorder StartRecording(RecordingParameters parameters)
        {
            return new SceneRecorder(
                parameters,
                _animators.ToArray(),
                _recorderBuilders.ToArray(),
                _scenePatches.ToArray()
            );
        }

        public Builder WithAnimator(PropertyAnimator animator)
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
    }
}
