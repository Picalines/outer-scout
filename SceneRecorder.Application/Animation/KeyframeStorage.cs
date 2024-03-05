using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.Application.Animation;

public sealed class KeyframeStorage<T>
{
    public IntRange FrameRange { get; }

    private readonly Keyframe<T>?[] _keyframes;

    private int _keyframeCount = 0;

    public KeyframeStorage(IntRange frameRange)
    {
        FrameRange = frameRange;

        var arrayLength = FrameRange.Length + 1;

        _keyframes = new Keyframe<T>?[arrayLength];
    }

    public bool IsEmpty
    {
        get => _keyframeCount is 0;
    }

    public void StoreKeyframe(Keyframe<T> keyframe)
    {
        var index = FrameRange.ValueToIndex(keyframe.Frame);

        if (_keyframes[index].HasValue is false)
        {
            _keyframeCount++;
        }

        _keyframes[index] = keyframe;
    }

    public bool RemoveKeyframe(int frame)
    {
        var index = FrameRange.ValueToIndex(frame);
        if (_keyframes[index].HasValue is false)
        {
            return false;
        }

        _keyframes[index] = null;
        _keyframeCount--;

        return true;
    }

    public bool HasKeyframe(int frame)
    {
        return _keyframes[FrameRange.ValueToIndex(frame)].HasValue;
    }

    public Keyframe<T>? GetKeyframeAt(int frame)
    {
        return _keyframes[FrameRange.ValueToIndex(frame)];
    }

    public Keyframe<T>? GetNextKeyframe(int frame)
    {
        return FindKeyframe(frame, 1);
    }

    public Keyframe<T>? GetPreviousKeyframe(int frame)
    {
        return FindKeyframe(frame, -1);
    }

    public Keyframe<T>? GetFirstKeyframe()
    {
        return FindKeyframe(FrameRange.Start, 1);
    }

    public Keyframe<T>? GetLastKeyframe()
    {
        return FindKeyframe(FrameRange.End, -1);
    }

    public (Keyframe<T> Left, Keyframe<T> Right)? GetRightSpan(int frame)
    {
        frame.Throw().If(!FrameRange.Contains(frame));

        if (IsEmpty)
        {
            return null;
        }

        var leftKeyframe = FindKeyframe(frame, -1);
        var rightKeyframe = FindKeyframe(frame, 1, 1) ?? GetKeyframeAt(frame);

        return (leftKeyframe, rightKeyframe) switch
        {
            ({ } left, { } right) => (left, right),
            (null, { } right) => (new Keyframe<T>(FrameRange.Start, right.Value), right),
            ({ } left, null) => (left, new Keyframe<T>(FrameRange.End, left.Value)),
            _ => throw new NotImplementedException(),
        };
    }

    private Keyframe<T>? FindKeyframe(int frame, int searchStep, int safeOffset = 0)
    {
        searchStep.Throw().IfEquals(0);

        if (IsEmpty)
        {
            return null;
        }

        int index = FrameRange.ValueToIndex(frame) + safeOffset;

        while (index >= 0 && index < _keyframes.Length)
        {
            if (_keyframes[index] is { } keyframe)
            {
                return keyframe;
            }

            index += searchStep;
        }

        return null;
    }
}
