using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Framework.Ownership.Models;

internal enum OwnershipWritePlanState
{
    Planned,
    Unchanged,

    /// <summary>
    /// The lock write did not happen and the command continues. The lock never
    /// gates, so there is no state in which it stops a command; a caller reports
    /// this and does its work.
    /// </summary>
    Skipped,
}

internal sealed record OwnershipWritePlanResult
{
    private const int MaximumCauseLength = 256;

    private OwnershipWritePlanResult(
        OwnershipWritePlanState state,
        PlannedFileChange? change,
        string? cause)
    {
        State = state;
        Change = change;
        Cause = cause;
    }

    internal OwnershipWritePlanState State { get; }

    internal PlannedFileChange? Change { get; }

    internal string? Cause { get; }

    internal static OwnershipWritePlanResult Planned(PlannedFileChange change)
    {
        ArgumentNullException.ThrowIfNull(change);
        return new OwnershipWritePlanResult(
            OwnershipWritePlanState.Planned,
            change,
            cause: null);
    }

    internal static OwnershipWritePlanResult Unchanged()
        => new(OwnershipWritePlanState.Unchanged, change: null, cause: null);

    internal static OwnershipWritePlanResult Skipped(string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return new OwnershipWritePlanResult(
            OwnershipWritePlanState.Skipped,
            change: null,
            cause.Length <= MaximumCauseLength ? cause : cause[..MaximumCauseLength]);
    }
}
