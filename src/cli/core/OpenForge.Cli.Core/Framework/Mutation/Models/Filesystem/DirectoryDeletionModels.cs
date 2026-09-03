namespace OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

internal enum DirectoryDeletionDisposition
{
    Removed,
    Retained,
    Unknown,
}

internal sealed record PlannedDirectoryDeletion
{
    private PlannedDirectoryDeletion(FileExpectation expectation)
    {
        Expectation = expectation;
    }

    internal FileExpectation Expectation { get; }

    internal string LogicalPath => Expectation.LogicalPath;

    internal static PlannedDirectoryDeletion DeleteIfEmpty(FileExpectation expectation)
    {
        ArgumentNullException.ThrowIfNull(expectation);
        if (expectation.Kind != FileExpectationKind.Directory)
        {
            throw new ArgumentException(
                "A directory expectation is required for a planned directory deletion.",
                nameof(expectation));
        }

        return new PlannedDirectoryDeletion(expectation);
    }
}

internal sealed record DirectoryDeletionReceiptState
{
    internal DirectoryDeletionReceiptState(
        FilesystemEffectState effectState,
        FilesystemVerificationState verificationState,
        FilesystemNotStartedReason? notStartedReason,
        DirectoryDeletionDisposition disposition)
    {
        if (!Enum.IsDefined(effectState))
        {
            throw new ArgumentOutOfRangeException(
                nameof(effectState),
                effectState,
                "The directory deletion effect state is not defined.");
        }

        if (!Enum.IsDefined(verificationState))
        {
            throw new ArgumentOutOfRangeException(
                nameof(verificationState),
                verificationState,
                "The directory deletion verification state is not defined.");
        }

        if (notStartedReason is { } reason && !Enum.IsDefined(reason))
        {
            throw new ArgumentOutOfRangeException(
                nameof(notStartedReason),
                notStartedReason,
                "The directory deletion not-started reason is not defined.");
        }

        if (!Enum.IsDefined(disposition))
        {
            throw new ArgumentOutOfRangeException(
                nameof(disposition),
                disposition,
                "The directory deletion disposition is not defined.");
        }

        if ((effectState == FilesystemEffectState.NotStarted) != notStartedReason.HasValue)
        {
            throw new ArgumentException(
                "A directory deletion not-started reason must match its effect state.",
                nameof(notStartedReason));
        }

        EffectState = effectState;
        VerificationState = verificationState;
        NotStartedReason = notStartedReason;
        Disposition = disposition;
    }

    internal FilesystemEffectState EffectState { get; }

    internal FilesystemVerificationState VerificationState { get; }

    internal FilesystemNotStartedReason? NotStartedReason { get; }

    internal DirectoryDeletionDisposition Disposition { get; }
}

internal sealed record DirectoryDeletionReceipt
{
    internal DirectoryDeletionReceipt(
        PlannedDirectoryDeletion deletion,
        FileStateSnapshot before,
        FileStateSnapshot? after,
        DirectoryDeletionReceiptState state,
        string? cause)
    {
        ArgumentNullException.ThrowIfNull(deletion);
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(state);
        if (before.Expectation != deletion.Expectation)
        {
            throw new ArgumentException(
                "A directory deletion receipt must retain its exact planned before-state.",
                nameof(before));
        }

        if (after is not null
            && !string.Equals(after.LogicalPath, deletion.LogicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A directory deletion after-state must describe the planned target.",
                nameof(after));
        }

        if (state.Disposition == DirectoryDeletionDisposition.Removed
            && (after is null || after.Kind != FileExpectationKind.Missing))
        {
            throw new ArgumentException(
                "A removed directory deletion must expose the verified missing after-state.",
                nameof(after));
        }

        if (state.Disposition == DirectoryDeletionDisposition.Retained
            && (after is null || after.Kind != FileExpectationKind.Directory))
        {
            throw new ArgumentException(
                "A retained directory deletion must expose the observed directory after-state.",
                nameof(after));
        }

        if (cause is not null && string.IsNullOrWhiteSpace(cause))
        {
            throw new ArgumentException(
                "A directory deletion cause cannot be empty.",
                nameof(cause));
        }

        Deletion = deletion;
        Before = before;
        After = after;
        State = state;
        Cause = cause;
    }

    internal PlannedDirectoryDeletion Deletion { get; }

    internal FileStateSnapshot Before { get; }

    internal FileStateSnapshot? After { get; }

    internal DirectoryDeletionReceiptState State { get; }

    internal string? Cause { get; }
}
