namespace SceneRecorder.WebApi.Http.Extensions;

internal static class TaskExtensions
{
    public static Task<T> AsCancellable<T>(this Task<T> task, CancellationToken token)
    {
        if (!token.CanBeCanceled)
        {
            return task;
        }

        var completionSource = new TaskCompletionSource<T>();
        token.Register(
            () => completionSource.TrySetCanceled(token),
            useSynchronizationContext: false
        );

        task.ContinueWith(
            t =>
            {
                if (task.IsCanceled)
                {
                    completionSource.TrySetCanceled();
                }
                else if (task.IsFaulted)
                {
                    completionSource.TrySetException(t.Exception);
                }
                else
                {
                    completionSource.TrySetResult(t.Result);
                }
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );

        return completionSource.Task;
    }
}
