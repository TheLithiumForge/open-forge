using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Models;

internal enum LifecycleWritePlanState
{
    Planned,
    Unchanged,
    Blocked,
}

internal sealed record LifecycleWritePlanResult
{
    private const int MaximumCauseLength = 256;

    private LifecycleWritePlanResult(
        LifecycleWritePlanState state,
        PlannedFileChange? change,
        string? cause)
    {
        State = state;
        Change = change;
        Cause = cause;
    }

    internal LifecycleWritePlanState State { get; }

    internal PlannedFileChange? Change { get; }

    internal string? Cause { get; }

    internal static LifecycleWritePlanResult Planned(PlannedFileChange change)
    {
        ArgumentNullException.ThrowIfNull(change);
        return new LifecycleWritePlanResult(
            LifecycleWritePlanState.Planned,
            change,
            cause: null);
    }

    internal static LifecycleWritePlanResult Unchanged()
        => new(LifecycleWritePlanState.Unchanged, change: null, cause: null);

    internal static LifecycleWritePlanResult Blocked(string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return new LifecycleWritePlanResult(
            LifecycleWritePlanState.Blocked,
            change: null,
            cause.Length <= MaximumCauseLength ? cause : cause[..MaximumCauseLength]);
    }
}
