using System.Collections;
using SceneRecorder.Shared;
using SceneRecorder.Shared.Validation;

namespace SceneRecorder.Recording.Animators;

public sealed class KeyframeStorage<T>
{
    public readonly record struct Keyframe(T Value, int Frame);

    public readonly record struct ValueSpan(IntRange FrameRange, T Left, T Right);

    public IntRange FrameRange { get; }

    private readonly T[] _keyframes;

    private readonly BitArray _keyframeFlags;

    private int _keyframeCount = 0;

    public KeyframeStorage(IntRange frameRange)
    {
        FrameRange = frameRange;

        var arrayLength = FrameRange.Length + 1;

        _keyframes = new T[arrayLength];
        _keyframeFlags = new BitArray(arrayLength, false);
    }

    public bool IsEmpty
    {
        get => _keyframeCount is 0;
    }

    public void SetKeyframe(int frame, in T value)
    {
        var index = FrameRange.ValueToIndex(frame);

        if (!_keyframeFlags[index])
        {
            _keyframeCount++;
        }

        _keyframes[index] = value;
        _keyframeFlags[index] = true;
    }

    public bool RemoveKeyframe(int frame)
    {
        var index = FrameRange.ValueToIndex(frame);
        if (!_keyframeFlags[index])
        {
            return false;
        }

        _keyframes[index] = default!;
        _keyframeFlags[index] = false;

        _keyframeCount--;

        if (IsEmpty)
        {
            throw new Exception();
        }

        return true;
    }

    public bool HasKeyframe(int frame)
    {
        return _keyframeFlags[FrameRange.ValueToIndex(frame)];
    }

    public Keyframe? GetKeyframeAt(int frame)
    {
        if (!HasKeyframe(frame))
        {
            return null;
        }

        var index = FrameRange.ValueToIndex(frame);

        return new(_keyframes[index], frame);
    }

    public Keyframe? GetNextKeyframe(int frame)
    {
        return FindKeyframe(frame, 1);
    }

    public Keyframe? GetPreviousKeyframe(int frame)
    {
        return FindKeyframe(frame, -1);
    }

    public Keyframe? GetFirstKeyframe()
    {
        return FindKeyframe(FrameRange.Start, 1);
    }

    public Keyframe? GetLastKeyframe()
    {
        return FindKeyframe(FrameRange.End, -1);
    }

    public ValueSpan? GetRightSpan(int frame)
    {
        frame.Throw().If(!FrameRange.Contains(frame));

        if (IsEmpty)
        {
            return null;
        }

        var leftKeyframe = FindKeyframe(frame, -1);
        var rightKeyframe = FindKeyframe(frame, 1, 1) ?? GetKeyframeAt(frame);

        // storage is not empty => only one frame can be null

        if (leftKeyframe is null)
        {
            leftKeyframe = rightKeyframe.GetValueOrDefault() with { Frame = FrameRange.Start };
        }
        else if (rightKeyframe is null)
        {
            rightKeyframe = leftKeyframe.GetValueOrDefault() with { Frame = FrameRange.End };
        }

        var (spanLeft, spanStart) = leftKeyframe.GetValueOrDefault();
        var (spanRight, spanEnd) = rightKeyframe.GetValueOrDefault();

        return new(new IntRange(spanStart, spanEnd), spanLeft, spanRight);
    }

    private Keyframe? FindKeyframe(int frame, int searchStep, int safeOffset = 0)
    {
        if (IsEmpty)
        {
            return null;
        }

        searchStep.Throw().IfEquals(0);

        int index = FrameRange.ValueToIndex(frame);

        index += safeOffset;

        while (index >= 0 && index < _keyframes.Length)
        {
            if (_keyframeFlags[index])
            {
                return new(_keyframes[index], frame);
            }

            index += searchStep;
            frame += searchStep;
        }

        return null;
    }
}
