using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Application;

internal enum RelativeFileLinkRecoveryState
{
    Restored,
    Mismatched,
    Blocked,
    Failed,
    Cancelled,
}

internal sealed record RelativeFileLinkRecoveryResult
{
    private RelativeFileLinkRecoveryResult(
        RelativeFileLinkRecoveryState state,
        RecoveryEntry entry,
        NoFollowLeafObservation current,
        NoFollowLeafObservation? after,
        FilesystemFailure? failure,
        string? cause)
    {
        State = state;
        Entry = entry;
        Current = current;
        After = after;
        Failure = failure;
        Cause = cause;
    }

    internal RelativeFileLinkRecoveryState State { get; }

    internal RecoveryEntry Entry { get; }

    internal NoFollowLeafObservation Current { get; }

    internal NoFollowLeafObservation? After { get; }

    internal FilesystemFailure? Failure { get; }

    internal string? Cause { get; }

    internal static RelativeFileLinkRecoveryResult Restored(
        RecoveryEntry entry,
        NoFollowLeafObservation current,
        NoFollowLeafObservation after)
        => Create(
            RelativeFileLinkRecoveryState.Restored,
            entry,
            current,
            after,
            failure: null,
            cause: null);

    internal static RelativeFileLinkRecoveryResult Classified(
        RelativeFileLinkRecoveryState state,
        RecoveryEntry entry,
        NoFollowLeafObservation current,
        NoFollowLeafObservation? after,
        string? cause = null,
        FilesystemFailure? failure = null)
    {
        if (state == RelativeFileLinkRecoveryState.Restored)
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        if (state is RelativeFileLinkRecoveryState.Blocked
            or RelativeFileLinkRecoveryState.Mismatched)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        }

        return Create(state, entry, current, after, failure, cause);
    }

    private static RelativeFileLinkRecoveryResult Create(
        RelativeFileLinkRecoveryState state,
        RecoveryEntry entry,
        NoFollowLeafObservation current,
        NoFollowLeafObservation? after,
        FilesystemFailure? failure,
        string? cause)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(current);
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The relative file-link recovery state is not defined.");
        }

        return new RelativeFileLinkRecoveryResult(
            state,
            entry,
            current,
            after,
            failure,
            cause);
    }
}
