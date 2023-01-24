#if IS_TARGET_MOD

using Newtonsoft.Json;

namespace Picalines.OuterWilds.SceneRecorder.Models;

public sealed class SceneTransformAnimationModel
{
    [JsonProperty("transforms")]
    public IReadOnlyList<TransformModel> Transforms { get; private set; } = null!;

    private SceneTransformAnimationModel()
    {
    }
}

#endif
