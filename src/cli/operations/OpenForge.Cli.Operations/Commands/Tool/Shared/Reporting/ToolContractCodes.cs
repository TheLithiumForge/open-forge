using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;

internal static class ToolContractCodes
{
    internal const string RestoreCommand = "open-forge tool install";
    internal const string StatusDeclaredToolMissing = "status.tool-declared-missing";
    internal const string DoctorDeclaredToolMissing = "doctor.tool-declared-missing";
    internal const string StatusEntryRewriteDrift = "status.tool-entry-rewrite-drift";
    internal const string DoctorEntryRewriteDrift = "doctor.tool-entry-rewrite-drift";

    internal static string Suffix(ToolWriteIssueCode code) => code switch
    {
        ToolWriteIssueCode.InvalidInput => "invalid-input",
        ToolWriteIssueCode.DeclarationMissing => "declaration-missing",
        ToolWriteIssueCode.LockMismatch => "lock-mismatch",
        ToolWriteIssueCode.PinNotSupported => "pin-not-supported",
        ToolWriteIssueCode.TargetUnavailable => "target-unavailable",
        ToolWriteIssueCode.DownloadConsentMissing => "download-consent-missing",
        ToolWriteIssueCode.ProcessConsentMissing => "process-consent-missing",
        ToolWriteIssueCode.PermissionMissing => "permission-missing",
        ToolWriteIssueCode.PathExcluded => "path-excluded",
        ToolWriteIssueCode.ComponentChoiceMissing => "component-choice-missing",
        ToolWriteIssueCode.EntryShapeUnexpected => "entry-shape-unexpected",
        ToolWriteIssueCode.EntryRewriteDrift => "entry-rewrite-drift",
        ToolWriteIssueCode.RoleGuidanceDrift => "role-guidance-drift",
        ToolWriteIssueCode.StoreUnavailable => "store-unavailable",
        ToolWriteIssueCode.CommandFailed => "command-failed",
        ToolWriteIssueCode.VerificationFailed => "verification-failed",
        ToolWriteIssueCode.BackupPresent => "backup-present",
        ToolWriteIssueCode.PublicationFailed => "publication-failed",
        ToolWriteIssueCode.Interrupted => "interrupted",
        ToolWriteIssueCode.OperationFailed => "operation-failed",
        ToolWriteIssueCode.ConfirmationMissing => "confirmation-missing",
        ToolWriteIssueCode.WorkspaceUnavailable => "workspace-unavailable",
        ToolWriteIssueCode.WorkspaceLockUnavailable => "workspace-lock-unavailable",
        _ => throw new ArgumentOutOfRangeException(nameof(code)),
    };
}
