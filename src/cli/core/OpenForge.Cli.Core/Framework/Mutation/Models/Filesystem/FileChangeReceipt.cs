using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

internal enum FileChangeEffectState
{
    NotStarted,
    Applied,
    Unknown,
}

internal enum FileChangeVerificationState
{
    NotStarted,
    Verified,
    Failed,
}

internal enum FileChangeNotStartedReason
{
    Cancelled,
    TargetChanged,
    ApplicationFailed,
    ContractRejected,
}

internal sealed record FileChangeReceipt
{
    private const int MaximumCauseLength = 256;

    private FileChangeReceipt(
        PlannedFileChange change,
        FileStateSnapshot before,
        FileStateSnapshot after)
    {
        ValidateBefore(change, before);
        ValidateVerifiedAfter(change, after);
        Change = change;
        Before = before;
        After = after;
        EffectState = FileChangeEffectState.Applied;
        VerificationState = FileChangeVerificationState.Verified;
    }

    private FileChangeReceipt(
        PlannedFileChange change,
        FileStateSnapshot before,
        FileStateSnapshot? after,
        string cause,
        FileChangeReceiptState state)
    {
        ValidateBefore(change, before);
        Change = change;
        Before = before;
        After = after;
        EffectState = state.EffectState;
        VerificationState = state.VerificationState;
        NotStartedReason = state.NotStartedReason;
        Cause = ValidateCause(cause);
    }

    internal PlannedFileChange Change { get; }

    internal FileStateSnapshot Before { get; }

    internal FileStateSnapshot? After { get; }

    internal FileChangeEffectState EffectState { get; }

    internal FileChangeVerificationState VerificationState { get; }

    internal FileChangeNotStartedReason? NotStartedReason { get; }

    internal string? Cause { get; }

    internal static FileChangeReceipt Verified(
        PlannedFileChange change,
        FileStateSnapshot before,
        FileStateSnapshot after)
    {
        return new FileChangeReceipt(
            change: change,
            before: before,
            after: after);
    }

    internal static FileChangeReceipt VerificationFailed(
        PlannedFileChange change,
        FileStateSnapshot before,
        FileStateSnapshot after,
        string cause)
    {
        ValidateVerificationFailedAfter(change, after);
        return new FileChangeReceipt(
            change: change,
            before: before,
            after: after,
            cause: cause,
            state: FileChangeReceiptState.VerificationFailed);
    }

    internal static FileChangeReceipt VerificationUnavailable(
        PlannedFileChange change,
        FileStateSnapshot before,
        string cause)
        => new(
            change: change,
            before: before,
            after: null,
            cause: cause,
            state: FileChangeReceiptState.VerificationFailed);

    internal static FileChangeReceipt NotStarted(
        PlannedFileChange change,
        FileStateSnapshot before,
        FileChangeNotStartedReason reason,
        string cause)
    {
        _ = reason switch
        {
            FileChangeNotStartedReason.Cancelled
                or FileChangeNotStartedReason.TargetChanged
                or FileChangeNotStartedReason.ApplicationFailed
                or FileChangeNotStartedReason.ContractRejected => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "The file-change not-started reason is not defined."),
        };

        return new FileChangeReceipt(
            change: change,
            before: before,
            after: null,
            cause: cause,
            state: FileChangeReceiptState.NotStarted(reason));
    }

    internal static FileChangeReceipt CompletionUnknown(
        PlannedFileChange change,
        FileStateSnapshot before,
        FileStateSnapshot? after,
        string cause)
    {
        ValidateCompletionUnknownAfter(change, before, after);
        return new FileChangeReceipt(
            change: change,
            before: before,
            after: after,
            cause: cause,
            state: FileChangeReceiptState.CompletionUnknown);
    }

    private static void ValidateBefore(
        PlannedFileChange change,
        FileStateSnapshot before)
    {
        ArgumentNullException.ThrowIfNull(change);
        ArgumentNullException.ThrowIfNull(before);
        if (before.Expectation != change.Expectation)
        {
            throw new ArgumentException(
                "A file receipt before-state must equal the planned expectation.",
                nameof(before));
        }
    }

    private static void ValidateVerifiedAfter(
        PlannedFileChange change,
        FileStateSnapshot after)
    {
        if (MatchesIntendedAfter(change, after))
        {
            return;
        }

        if (change.Kind == PlannedFileChangeKind.Delete)
        {
            throw new ArgumentException(
                "A verified delete requires a missing after-state.",
                nameof(after));
        }

        throw new ArgumentException(
            "A verified write requires the exact intended after-bytes.",
            nameof(after));
    }

    private static void ValidateVerificationFailedAfter(
        PlannedFileChange change,
        FileStateSnapshot after)
    {
        if (MatchesIntendedAfter(change, after))
        {
            throw new ArgumentException(
                "A verification-failed receipt cannot carry the exact intended after-state.",
                nameof(after));
        }
    }

    private static void ValidateCompletionUnknownAfter(
        PlannedFileChange change,
        FileStateSnapshot before,
        FileStateSnapshot? after)
    {
        if (after is null)
        {
            return;
        }

        if (MatchesIntendedAfter(change, after))
        {
            throw new ArgumentException(
                "An unknown-completion receipt cannot carry the exact intended after-state.",
                nameof(after));
        }

        if (after.Expectation == before.Expectation)
        {
            throw new ArgumentException(
                "An unknown-completion receipt cannot carry the exact unchanged before-state.",
                nameof(after));
        }
    }

    private static bool MatchesIntendedAfter(
        PlannedFileChange change,
        FileStateSnapshot after)
    {
        ValidateAfterTarget(change, after);
        return change.Kind switch
        {
            PlannedFileChangeKind.Create => MatchesIntendedFileBytes(change, after),
            PlannedFileChangeKind.Replace
                or PlannedFileChangeKind.ReplaceGeneratedRegion => MatchesIntendedFileBytes(change, after)
                    && MatchesPlannedPhysicalIdentity(change, after),
            PlannedFileChangeKind.Delete => after.Kind == FileExpectationKind.Missing,
            _ => throw new ArgumentOutOfRangeException(
                nameof(change),
                change.Kind,
                "The planned file change kind is not defined."),
        };
    }

    private static bool MatchesIntendedFileBytes(
        PlannedFileChange change,
        FileStateSnapshot after)
        => after.Kind == FileExpectationKind.File
            && after.HasBytes
            && after.Bytes.AsSpan().SequenceEqual(change.IntendedBytes.AsSpan());

    private static bool MatchesPlannedPhysicalIdentity(
        PlannedFileChange change,
        FileStateSnapshot after)
        => change.Expectation.PhysicalPath is { } plannedPhysicalPath
            && after.PhysicalPath is { } afterPhysicalPath
            && PhysicalIdentityTracker.PathComparer.Equals(
                plannedPhysicalPath,
                afterPhysicalPath);

    private static void ValidateAfterTarget(
        PlannedFileChange change,
        FileStateSnapshot after)
    {
        ArgumentNullException.ThrowIfNull(after);
        if (!string.Equals(change.LogicalPath, after.LogicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A file receipt after-state must describe the planned logical target.",
                nameof(after));
        }
    }

    private static string ValidateCause(string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return cause.Length <= MaximumCauseLength
            ? cause
            : cause[..MaximumCauseLength];
    }

    private sealed record FileChangeReceiptState
    {
        private FileChangeReceiptState(
            FileChangeEffectState effectState,
            FileChangeVerificationState verificationState,
            FileChangeNotStartedReason? notStartedReason)
        {
            EffectState = effectState;
            VerificationState = verificationState;
            NotStartedReason = notStartedReason;
        }

        public FileChangeEffectState EffectState { get; }

        public FileChangeVerificationState VerificationState { get; }

        public FileChangeNotStartedReason? NotStartedReason { get; }

        public static FileChangeReceiptState VerificationFailed { get; } = new(
            FileChangeEffectState.Applied,
            FileChangeVerificationState.Failed,
            notStartedReason: null);

        public static FileChangeReceiptState CompletionUnknown { get; } = new(
            FileChangeEffectState.Unknown,
            FileChangeVerificationState.NotStarted,
            notStartedReason: null);

        public static FileChangeReceiptState NotStarted(FileChangeNotStartedReason reason)
            => new(
                FileChangeEffectState.NotStarted,
                FileChangeVerificationState.NotStarted,
                reason);
    }
}
