using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Library.Sync;

internal static class LibrarySyncDefinitions
{
    internal const string CommandIdentity = "library sync";
    internal static readonly CliSyntaxDefinition Command = new("sync", "Synchronize one registered Library from its complete inventory.");
    internal static string ReadFindingCode(LibrarySyncFindingCode value)
        => value switch
        {
            LibrarySyncFindingCode.PermissionRequired => "library-sync.permission-required",
            LibrarySyncFindingCode.PermissionDeclined => "library-sync.permission-declined",
            LibrarySyncFindingCode.PermissionInvalid => "library-sync.permission-invalid",
            LibrarySyncFindingCode.PermissionUnavailable => "library-sync.permission-unavailable",
            LibrarySyncFindingCode.PermissionChanged => "library-sync.permission-changed",
            LibrarySyncFindingCode.PermissionWriteFailed => "library-sync.permission-write-failed",
            LibrarySyncFindingCode.InvalidInput => "library-sync.invalid-input",
            LibrarySyncFindingCode.InvalidId => "library-sync.invalid-id",
            LibrarySyncFindingCode.RecordInvalid => "library-sync.record-invalid",
            LibrarySyncFindingCode.RecordUnavailable => "library-sync.record-unavailable",
            LibrarySyncFindingCode.RecordBlocked => "library-sync.record-blocked",
            LibrarySyncFindingCode.ConsumerBlocked => "library-sync.consumer-blocked",
            LibrarySyncFindingCode.MappingBlocked => "library-sync.mapping-blocked",
            LibrarySyncFindingCode.MappingUnavailable => "library-sync.mapping-unavailable",
            LibrarySyncFindingCode.OwnershipConflict => "library-sync.ownership-conflict",
            LibrarySyncFindingCode.GeneratedNavigationIncomplete => "library-sync.generated-navigation-incomplete",
            LibrarySyncFindingCode.GeneratedNavigationBlocked => "library-sync.generated-navigation-blocked",
            LibrarySyncFindingCode.LinkCapabilityUnavailable => "library-sync.link-capability-unavailable",
            LibrarySyncFindingCode.LockUnavailable => "library-sync.lock-unavailable",
            LibrarySyncFindingCode.RecoveryUnavailable => "library-sync.recovery-unavailable",
            LibrarySyncFindingCode.ApplicationFailed => "library-sync.application-failed",
            LibrarySyncFindingCode.VerificationFailed => "library-sync.verification-failed",
            LibrarySyncFindingCode.RecoveryRetained => "library-sync.recovery-retained",
            LibrarySyncFindingCode.OperationFailed => "library-sync.operation-failed",
            LibrarySyncFindingCode.Interrupted => "library-sync.interrupted",
            LibrarySyncFindingCode.UnknownId => "library-sync.unknown-id",
            LibrarySyncFindingCode.SourceRootInvalid => "library-sync.source-root-invalid",
            LibrarySyncFindingCode.SourceRootUnavailable => "library-sync.source-root-unavailable",
            LibrarySyncFindingCode.SourceRootBlocked => "library-sync.source-root-blocked",
            LibrarySyncFindingCode.InventoryIncomplete => "library-sync.inventory-incomplete",
            LibrarySyncFindingCode.DestinationCollision => "library-sync.destination-collision",
            LibrarySyncFindingCode.RetiredLinkMissing => "library-sync.retired-link-missing",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library finding code is not defined."),
        };
}
