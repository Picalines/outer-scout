using OuterScout.Shared.Collections;

namespace OuterScout.Application.Recording;

public sealed partial class SceneRecorder
{
    public sealed class RecordingParameters
    {
        public required IntRange FrameRange { get; init; }

        public required int FrameRate { get; init; }
    }
}
