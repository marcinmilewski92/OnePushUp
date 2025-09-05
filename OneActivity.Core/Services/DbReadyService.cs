namespace OneActivity.Core.Services;

public class DbReadyService
{
    private readonly TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool IsReady => _tcs.Task.IsCompleted;

    public void MarkReady()
    {
        // Idempotent
        _tcs.TrySetResult(true);
    }

    public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
    {
        if (!cancellationToken.CanBeCanceled)
            return _tcs.Task;

        // If a token is provided, return a task that completes when either the DB is ready or the token is canceled.
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = Task.WhenAny(_tcs.Task, Task.Run(async () =>
        {
            try { await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken); }
            catch { }
        })).ContinueWith(_ =>
        {
            if (_tcs.Task.IsCompleted)
                tcs.TrySetResult();
            else
                tcs.TrySetCanceled(cancellationToken);
        }, TaskScheduler.Default);
        return tcs.Task;
    }
}

