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

internal sealed record FileChangeReceipt
{
    private const int MaximumCauseLength = 256;

    private FileChangeReceipt(
        PlannedFileChange change,
        FileStateSnapshot before,
        FileStateSnapshot? after,
        FileChangeEffectState effectState,
        FileChangeVerificationState verificationState,
        string? cause)
    {
        Change = change;
        Before = before;
        After = after;
        EffectState = effectState;
        VerificationState = verificationState;
        Cause = cause;
    }

    internal PlannedFileChange Change { get; }

    internal FileStateSnapshot Before { get; }

    internal FileStateSnapshot? After { get; }

    internal FileChangeEffectState EffectState { get; }

    internal FileChangeVerificationState VerificationState { get; }

    internal string? Cause { get; }

    internal static FileChangeReceipt Verified(
        PlannedFileChange change,
        FileStateSnapshot before,
        FileStateSnapshot after)
    {
        ValidateBefore(change, before);
        ValidateVerifiedAfter(change, after);
        return new FileChangeReceipt(
            change: change,
            before: before,
            after: after,
            effectState: FileChangeEffectState.Applied,
            verificationState: FileChangeVerificationState.Verified,
            cause: null);
    }

    internal static FileChangeReceipt VerificationFailed(
        PlannedFileChange change,
        FileStateSnapshot before,
        FileStateSnapshot after,
        string cause)
    {
        ValidateBefore(change, before);
        ValidateAfterTarget(change, after);
        return new FileChangeReceipt(
            change: change,
            before: before,
            after: after,
            effectState: FileChangeEffectState.Applied,
            verificationState: FileChangeVerificationState.Failed,
            cause: ValidateCause(cause));
    }

    internal static FileChangeReceipt NotStarted(
        PlannedFileChange change,
        FileStateSnapshot before,
        string cause)
    {
        ValidateBefore(change, before);
        return new FileChangeReceipt(
            change: change,
            before: before,
            after: null,
            effectState: FileChangeEffectState.NotStarted,
            verificationState: FileChangeVerificationState.NotStarted,
            cause: ValidateCause(cause));
    }

    internal static FileChangeReceipt CompletionUnknown(
        PlannedFileChange change,
        FileStateSnapshot before,
        FileStateSnapshot? after,
        string cause)
    {
        ValidateBefore(change, before);
        if (after is not null)
        {
            ValidateAfterTarget(change, after);
        }

        return new FileChangeReceipt(
            change: change,
            before: before,
            after: after,
            effectState: FileChangeEffectState.Unknown,
            verificationState: FileChangeVerificationState.NotStarted,
            cause: ValidateCause(cause));
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
        ValidateAfterTarget(change, after);
        if (change.Kind == PlannedFileChangeKind.Delete)
        {
            if (after.Kind != FileExpectationKind.Missing)
            {
                throw new ArgumentException(
                    "A verified delete requires a missing after-state.",
                    nameof(after));
            }

            return;
        }

        if (after.Kind != FileExpectationKind.File
            || !after.HasBytes
            || !after.Bytes.AsSpan().SequenceEqual(change.IntendedBytes.AsSpan()))
        {
            throw new ArgumentException(
                "A verified write requires the exact intended after-bytes.",
                nameof(after));
        }
    }

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
}
