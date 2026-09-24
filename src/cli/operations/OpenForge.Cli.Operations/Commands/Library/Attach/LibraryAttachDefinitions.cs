using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Library.Attach;

internal static class LibraryAttachDefinitions
{
    internal const string CommandIdentity = "library attach";
    internal static readonly CliSyntaxDefinition Command = new("attach", "Register one Library source and create relative file links.");
    internal static readonly CliOptionDefinition<bool> Automatic = new(
        "--automatic",
        "Disable prompts; does not select or alter Library targets.",
        CliOptionArity.None,
        false);
    internal static string ReadFindingCode(LibraryAttachFindingCode value)
        => value switch
        {
            LibraryAttachFindingCode.DestinationRootInvalid => "library-attach.destination-root-invalid",
            LibraryAttachFindingCode.PermissionRequired => "library-attach.permission-required",
            LibraryAttachFindingCode.PermissionDeclined => "library-attach.permission-declined",
            LibraryAttachFindingCode.PermissionInvalid => "library-attach.permission-invalid",
            LibraryAttachFindingCode.PermissionUnavailable => "library-attach.permission-unavailable",
            LibraryAttachFindingCode.PermissionChanged => "library-attach.permission-changed",
            LibraryAttachFindingCode.PermissionWriteFailed => "library-attach.permission-write-failed",
            LibraryAttachFindingCode.InvalidInput => "library-attach.invalid-input",
            LibraryAttachFindingCode.InvalidId => "library-attach.invalid-id",
            LibraryAttachFindingCode.RecordInvalid => "library-attach.record-invalid",
            LibraryAttachFindingCode.OwnershipObservation => "library-attach.ownership-observation",
            LibraryAttachFindingCode.RecordUnavailable => "library-attach.record-unavailable",
            LibraryAttachFindingCode.RecordBlocked => "library-attach.record-blocked",
            LibraryAttachFindingCode.ConsumerBlocked => "library-attach.consumer-blocked",
            LibraryAttachFindingCode.MappingBlocked => "library-attach.mapping-blocked",
            LibraryAttachFindingCode.MappingUnavailable => "library-attach.mapping-unavailable",
            LibraryAttachFindingCode.OwnershipConflict => "library-attach.ownership-conflict",
            LibraryAttachFindingCode.GeneratedNavigationIncomplete => "library-attach.generated-navigation-incomplete",
            LibraryAttachFindingCode.GeneratedNavigationBlocked => "library-attach.generated-navigation-blocked",
            LibraryAttachFindingCode.LinkCapabilityUnavailable => "library-attach.link-capability-unavailable",
            LibraryAttachFindingCode.LockUnavailable => "library-attach.lock-unavailable",
            LibraryAttachFindingCode.RecoveryUnavailable => "library-attach.recovery-unavailable",
            LibraryAttachFindingCode.ApplicationFailed => "library-attach.application-failed",
            LibraryAttachFindingCode.VerificationFailed => "library-attach.verification-failed",
            LibraryAttachFindingCode.RecoveryRetained => "library-attach.recovery-retained",
            LibraryAttachFindingCode.OperationFailed => "library-attach.operation-failed",
            LibraryAttachFindingCode.Interrupted => "library-attach.interrupted",
            LibraryAttachFindingCode.DuplicateId => "library-attach.duplicate-id",
            LibraryAttachFindingCode.SourceRootInvalid => "library-attach.source-root-invalid",
            LibraryAttachFindingCode.SourceRootUnavailable => "library-attach.source-root-unavailable",
            LibraryAttachFindingCode.SourceRootBlocked => "library-attach.source-root-blocked",
            LibraryAttachFindingCode.InventoryIncomplete => "library-attach.inventory-incomplete",
            LibraryAttachFindingCode.DestinationCollision => "library-attach.destination-collision",
            LibraryAttachFindingCode.LibraryRemoved => "library-attach.library-removed",
            LibraryAttachFindingCode.PathExcluded => "library-attach.path-excluded",
            LibraryAttachFindingCode.ConfirmationRequired => "library-attach.confirmation-required",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library finding code is not defined."),
        };
}
