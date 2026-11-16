using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Result;

namespace OpenForge.Cli.Core.Commands.Shared.Stripping;

/// <summary>
/// Public vocabulary shared by strip and uninstall. The strip Interface contract
/// defines the part names, plan fields and finding codes for both commands.
/// </summary>
internal static class StrippingDefinitions
{
    internal const string StripCommandIdentity = "strip";
    internal const string UninstallCommandIdentity = "uninstall";
    internal const char PartSeparator = ',';
    internal const string StripOption = "--strip";
    internal const string FlattenOption = "--flatten";
    internal const string RemoveEditedOption = "--remove-edited";
    internal const string DryRunOption = "--dry-run";
    internal const string AutomaticOption = "--automatic";

    internal static string CommandIdentity(StripCommandKind kind)
        => kind switch
        {
            StripCommandKind.Strip => StripCommandIdentity,
            StripCommandKind.Uninstall => UninstallCommandIdentity,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The strip command kind is not defined."),
        };

    internal static string PartName(StripPart part)
        => part switch
        {
            StripPart.Metadata => "metadata",
            StripPart.Entries => "entries",
            StripPart.Links => "links",
            _ => throw new ArgumentOutOfRangeException(nameof(part), part, "The strip part is not defined."),
        };

    internal static string RewritePartName(StripRewritePart part)
        => part switch
        {
            StripRewritePart.Metadata => "metadata",
            StripRewritePart.Entries => "entries",
            StripRewritePart.Links => "links",
            StripRewritePart.Relink => "relink",
            StripRewritePart.ManagedSection => "managed-section",
            _ => throw new ArgumentOutOfRangeException(nameof(part), part, "The strip rewrite part is not defined."),
        };

    internal static string ActionName(StripPlanAction action)
        => action switch
        {
            StripPlanAction.Delete => "delete",
            StripPlanAction.Rewrite => "rewrite",
            StripPlanAction.Move => "move",
            StripPlanAction.DeleteDirectory => "delete-directory",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "The strip plan action is not defined."),
        };

    internal static string ReasonName(StripPlanReason reason)
        => reason switch
        {
            StripPlanReason.Framework => "framework",
            StripPlanReason.Extension => "extension",
            StripPlanReason.RootHost => "root-host",
            StripPlanReason.Scaffold => "scaffold",
            StripPlanReason.State => "state",
            StripPlanReason.Strip => "strip",
            StripPlanReason.Flatten => "flatten",
            StripPlanReason.EmptyFolder => "empty-folder",
            _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "The strip plan reason is not defined."),
        };

    internal static string KeptReasonName(StripKeptReason reason)
        => reason switch
        {
            StripKeptReason.Edited => "edited",
            _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "The strip kept reason is not defined."),
        };

    internal static string FindingCode(StripCommandKind kind, StripFindingCode code)
        => $"{CommandIdentity(kind)}.{FindingSuffix(code)}";

    internal static string FindingSuffix(StripFindingCode code)
        => code switch
        {
            StripFindingCode.InvalidInput => "invalid-input",
            StripFindingCode.WorkspaceUnavailable => "workspace-unavailable",
            StripFindingCode.DocumentUnsupported => "document-unsupported",
            StripFindingCode.FlattenCollision => "flatten-collision",
            StripFindingCode.LinkedFileSkipped => "linked-file-skipped",
            StripFindingCode.TargetChanged => "target-changed",
            StripFindingCode.WorkspaceLockUnavailable => "workspace-lock-unavailable",
            StripFindingCode.ConfirmationRequired => "confirmation-required",
            StripFindingCode.PermissionDeclined => "permission-declined",
            StripFindingCode.RecoveryUnavailable => "recovery-unavailable",
            StripFindingCode.WriteFailed => "write-failed",
            StripFindingCode.RecoveryArtifactRetained => "recovery-artifact-retained",
            StripFindingCode.Interrupted => "interrupted",
            StripFindingCode.OwnershipUnavailable => "ownership-unavailable",
            StripFindingCode.LibraryAttached => "library-attached",
            StripFindingCode.TargetUnavailable => "target-unavailable",
            StripFindingCode.EditedKept => "edited-kept",
            StripFindingCode.RuntimeRetained => "runtime-retained",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The strip finding code is not defined."),
        };
}
