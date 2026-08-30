using OpenForge.Cli.Core.Framework.Filesystem;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

internal enum WorkspaceLockState
{
    Acquired,
    Failed,
    Cancelled,
}

internal enum WorkspaceLockBootstrapOutcome
{
    Existing,
    Materialized,
}

internal sealed record WorkspaceLockResult
{
    private WorkspaceLockResult(
        WorkspaceLockState state,
        WorkspaceLockLease? lease,
        FilesystemFailure? failure,
        string? cause,
        WorkspaceLockBootstrapOutcome? bootstrapOutcome)
    {
        State = state;
        Lease = lease;
        Failure = failure;
        Cause = cause;
        BootstrapOutcome = ValidateBootstrapOutcome(bootstrapOutcome);
    }

    internal WorkspaceLockState State { get; }

    internal WorkspaceLockLease? Lease { get; }

    internal WorkspaceLockBootstrapOutcome? BootstrapOutcome { get; }

    internal FilesystemFailure? Failure { get; }

    internal string? Cause { get; }

    internal static WorkspaceLockResult Acquired(
        WorkspaceLockLease? lease,
        WorkspaceLockBootstrapOutcome bootstrapOutcome)
    {
        ArgumentNullException.ThrowIfNull(lease);
        if (!lease.IsHeld)
        {
            throw new ArgumentException("An acquired lock result requires a held lease.", nameof(lease));
        }

        return new WorkspaceLockResult(
            state: WorkspaceLockState.Acquired,
            lease: lease,
            failure: null,
            cause: null,
            bootstrapOutcome: bootstrapOutcome);
    }

    internal static WorkspaceLockResult Failed(
        FilesystemFailure failure,
        WorkspaceLockBootstrapOutcome? bootstrapOutcome = null)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new WorkspaceLockResult(
            state: WorkspaceLockState.Failed,
            lease: null,
            failure: failure,
            cause: failure.DirectCause,
            bootstrapOutcome: bootstrapOutcome);
    }

    internal static WorkspaceLockResult Cancelled(
        WorkspaceLockBootstrapOutcome? bootstrapOutcome = null)
        => new(
            state: WorkspaceLockState.Cancelled,
            lease: null,
            failure: null,
            cause: null,
            bootstrapOutcome: bootstrapOutcome);

    private static WorkspaceLockBootstrapOutcome? ValidateBootstrapOutcome(
        WorkspaceLockBootstrapOutcome? bootstrapOutcome)
    {
        if (bootstrapOutcome is null)
        {
            return null;
        }

        return bootstrapOutcome.Value switch
        {
            WorkspaceLockBootstrapOutcome.Existing => WorkspaceLockBootstrapOutcome.Existing,
            WorkspaceLockBootstrapOutcome.Materialized => WorkspaceLockBootstrapOutcome.Materialized,
            _ => throw new ArgumentOutOfRangeException(
                nameof(bootstrapOutcome),
                bootstrapOutcome,
                "The workspace lock bootstrap outcome is not defined."),
        };
    }
}
