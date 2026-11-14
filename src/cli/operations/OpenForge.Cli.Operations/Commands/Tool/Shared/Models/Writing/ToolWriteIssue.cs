using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;

internal enum ToolWriteIssueCode
{
    InvalidInput, DeclarationMissing, LockMismatch, PinNotSupported, TargetUnavailable,
    DownloadConsentMissing, ProcessConsentMissing, PermissionMissing, PathExcluded,
    ComponentChoiceMissing, EntryShapeUnexpected, EntryRewriteDrift, RoleGuidanceDrift,
    StoreUnavailable, CommandFailed, VerificationFailed, BackupPresent, PublicationFailed,
    Interrupted, OperationFailed,
    ConfirmationMissing, WorkspaceUnavailable, WorkspaceLockUnavailable,
}

internal sealed record ToolWriteIssue(ToolWriteIssueCode Code, CliSemanticStatus Status, string Subject, string Cause);
