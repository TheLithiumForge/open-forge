namespace OpenForge.Cli.Composition.Shared.Git;

/// <summary>
/// Gives each Git invocation its own deadline, as the forgiving-maintenance technical design requires.
/// A caller that runs several invocations shares no time budget across them.
/// </summary>
internal sealed class GitInvocationDeadline(TimeProvider? timeProvider = null)
{
    internal static readonly TimeSpan Duration = TimeSpan.FromSeconds(5);
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    /// <summary>
    /// Runs one invocation under a fresh deadline linked to the caller's token.
    /// An expired deadline throws <see cref="OperationCanceledException"/> after the runner has reaped the child.
    /// </summary>
    internal async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> invocation, CancellationToken cancellationToken)
    {
        using var timeout = new CancellationTokenSource(Duration, _timeProvider);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        var started = _timeProvider.GetTimestamp();
        deadline.Token.ThrowIfCancellationRequested();
        var result = await invocation(deadline.Token).ConfigureAwait(false);
        if (_timeProvider.GetElapsedTime(started) >= Duration) timeout.Cancel();
        deadline.Token.ThrowIfCancellationRequested();
        return result;
    }
}
