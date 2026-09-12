using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;

namespace OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;

internal sealed partial record DirectoryCreationReceipt
{
    private DirectoryCreationReceipt(
        PlannedDirectoryCreation creation,
        FileStateSnapshot before,
        string intendedPhysicalPath,
        FileStateSnapshot after)
    {
        ValidateBefore(creation, before);
        IntendedPhysicalPath = ValidateIntendedPhysicalPath(intendedPhysicalPath);
        ValidateVerifiedAfter(creation, IntendedPhysicalPath, after);
        Creation = creation;
        Before = before;
        After = after;
        EffectState = FilesystemEffectState.Applied;
        VerificationState = FilesystemVerificationState.Verified;
    }

    private DirectoryCreationReceipt(
        PlannedDirectoryCreation creation,
        FileStateSnapshot before,
        string intendedPhysicalPath,
        FileStateSnapshot? after,
        string cause,
        FilesystemReceiptState state)
    {
        ValidateBefore(creation, before);
        Creation = creation;
        Before = before;
        IntendedPhysicalPath = ValidateIntendedPhysicalPath(intendedPhysicalPath);
        After = after;
        EffectState = state.EffectState;
        VerificationState = state.VerificationState;
        NotStartedReason = state.NotStartedReason;
        Cause = ValidateCause(cause);
    }

    internal PlannedDirectoryCreation Creation { get; }

    internal FileStateSnapshot Before { get; }

    internal string IntendedPhysicalPath { get; }

    internal FileStateSnapshot? After { get; }

    internal FilesystemEffectState EffectState { get; }

    internal FilesystemVerificationState VerificationState { get; }

    internal FilesystemNotStartedReason? NotStartedReason { get; }

    internal string? Cause { get; }

    internal static DirectoryCreationReceipt Verified(
        PlannedDirectoryCreation creation,
        FileStateSnapshot before,
        string intendedPhysicalPath,
        FileStateSnapshot after)
        => new(
            creation,
            before,
            intendedPhysicalPath,
            after);

    internal static DirectoryCreationReceipt VerificationFailed(
        PlannedDirectoryCreation creation,
        FileStateSnapshot before,
        string intendedPhysicalPath,
        FileStateSnapshot after,
        string cause)
    {
        ValidateVerificationFailedAfter(creation, intendedPhysicalPath, after);
        return new DirectoryCreationReceipt(
            creation,
            before,
            intendedPhysicalPath,
            after,
            cause,
            FilesystemReceiptState.VerificationFailed);
    }

    internal static DirectoryCreationReceipt VerificationUnavailable(
        PlannedDirectoryCreation creation,
        FileStateSnapshot before,
        string intendedPhysicalPath,
        string cause)
        => new(
            creation,
            before,
            intendedPhysicalPath,
            after: null,
            cause,
            FilesystemReceiptState.VerificationFailed);

    internal static DirectoryCreationReceipt NotStarted(
        PlannedDirectoryCreation creation,
        FileStateSnapshot before,
        string intendedPhysicalPath,
        FilesystemNotStartedReason reason,
        string cause)
    {
        ValidateNotStartedReason(reason);
        return new DirectoryCreationReceipt(
            creation,
            before,
            intendedPhysicalPath,
            after: null,
            cause,
            FilesystemReceiptState.NotStarted(reason));
    }

    internal static DirectoryCreationReceipt CompletionUnknown(
        PlannedDirectoryCreation creation,
        FileStateSnapshot before,
        string intendedPhysicalPath,
        FileStateSnapshot? after,
        string cause)
    {
        ValidateCompletionUnknownAfter(
            creation,
            before,
            intendedPhysicalPath,
            after);
        return new DirectoryCreationReceipt(
            creation,
            before,
            intendedPhysicalPath,
            after,
            cause,
            FilesystemReceiptState.CompletionUnknown);
    }
}
