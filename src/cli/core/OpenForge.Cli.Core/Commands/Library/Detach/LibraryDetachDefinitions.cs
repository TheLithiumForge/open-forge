using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Library.Detach;

internal static class LibraryDetachDefinitions
{
    internal const string CommandIdentity = "library detach";
    internal static readonly CliSyntaxDefinition Command = new("detach", "Remove a Library registration and its links; preserve source files.");
    internal static string ReadFindingCode(LibraryDetachFindingCode value)
        => value switch
        {
            LibraryDetachFindingCode.PermissionRequired => "library-detach.permission-required",
            LibraryDetachFindingCode.PermissionDeclined => "library-detach.permission-declined",
            LibraryDetachFindingCode.PermissionInvalid => "library-detach.permission-invalid",
            LibraryDetachFindingCode.PermissionUnavailable => "library-detach.permission-unavailable",
            LibraryDetachFindingCode.PermissionChanged => "library-detach.permission-changed",
            LibraryDetachFindingCode.PermissionWriteFailed => "library-detach.permission-write-failed",
            LibraryDetachFindingCode.InvalidInput => "library-detach.invalid-input",
            LibraryDetachFindingCode.InvalidId => "library-detach.invalid-id",
            LibraryDetachFindingCode.RecordInvalid => "library-detach.record-invalid",
            LibraryDetachFindingCode.RecordUnavailable => "library-detach.record-unavailable",
            LibraryDetachFindingCode.RecordBlocked => "library-detach.record-blocked",
            LibraryDetachFindingCode.ConsumerBlocked => "library-detach.consumer-blocked",
            LibraryDetachFindingCode.MappingBlocked => "library-detach.mapping-blocked",
            LibraryDetachFindingCode.MappingUnavailable => "library-detach.mapping-unavailable",
            LibraryDetachFindingCode.OwnershipConflict => "library-detach.ownership-conflict",
            LibraryDetachFindingCode.GeneratedNavigationIncomplete => "library-detach.generated-navigation-incomplete",
            LibraryDetachFindingCode.GeneratedNavigationBlocked => "library-detach.generated-navigation-blocked",
            LibraryDetachFindingCode.LinkCapabilityUnavailable => "library-detach.link-capability-unavailable",
            LibraryDetachFindingCode.LockUnavailable => "library-detach.lock-unavailable",
            LibraryDetachFindingCode.RecoveryUnavailable => "library-detach.recovery-unavailable",
            LibraryDetachFindingCode.ApplicationFailed => "library-detach.application-failed",
            LibraryDetachFindingCode.VerificationFailed => "library-detach.verification-failed",
            LibraryDetachFindingCode.RecoveryRetained => "library-detach.recovery-retained",
            LibraryDetachFindingCode.OperationFailed => "library-detach.operation-failed",
            LibraryDetachFindingCode.Interrupted => "library-detach.interrupted",
            LibraryDetachFindingCode.UnknownId => "library-detach.unknown-id",
            LibraryDetachFindingCode.RegisteredLinkMissing => "library-detach.registered-link-missing",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library finding code is not defined."),
        };
}
