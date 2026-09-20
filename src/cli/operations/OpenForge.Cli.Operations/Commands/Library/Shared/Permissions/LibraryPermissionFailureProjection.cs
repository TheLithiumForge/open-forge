using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Permissions;

internal static class LibraryPermissionFailureProjection
{
    internal static (CliSemanticStatus Status, string Cause) Read(LibraryPermissionFailure failure)
        => failure switch
        {
            LibraryPermissionFailure.Required => (CliSemanticStatus.Blocked, "The selected Library destinations require shared permission in .agents/open-forge.json; use --allow-path <path> to record a grant."),
            LibraryPermissionFailure.Declined => (CliSemanticStatus.Interrupted, "Library permission admission was cancelled. Nothing was changed."),
            LibraryPermissionFailure.Invalid => (CliSemanticStatus.Blocked, "The workspace settings file is invalid or blocked."),
            LibraryPermissionFailure.Unavailable => (CliSemanticStatus.Blocked, "The workspace settings file is unavailable."),
            LibraryPermissionFailure.Changed => (CliSemanticStatus.Blocked, "The reviewed Library plan or permission observation changed before application."),
            LibraryPermissionFailure.WriteFailed => (CliSemanticStatus.Failed, "The settings grant write was refused or could not be verified."),
            LibraryPermissionFailure.Interrupted => (CliSemanticStatus.Interrupted, "Library permission admission or publication was interrupted."),
            _ => throw new ArgumentOutOfRangeException(nameof(failure), failure, "The Library permission failure is not defined."),
        };
}
