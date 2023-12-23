namespace SceneRecorder.Recording;

public static class AnimatorExtensions
{
    public static int GetFrameCount(this IAnimator animator)
    {
        return animator.EndFrame - animator.StartFrame + 1;
    }

    public static IEnumerable<int> GetFrameNumbers(this IAnimator animator)
    {
        return Enumerable.Range(animator.StartFrame, animator.GetFrameCount());
    }
}
