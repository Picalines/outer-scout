namespace OuterScout.Application.Animation;

public readonly record struct Keyframe<T>(int Frame, T Value);
