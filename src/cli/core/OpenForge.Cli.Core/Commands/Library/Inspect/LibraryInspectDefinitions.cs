using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Library.Inspect;

internal static class LibraryInspectDefinitions
{
    internal const string CommandIdentity = "library inspect";
    internal static readonly CliSyntaxDefinition Command = new("inspect", "Inspect all source files and destination links for one Library.");
    internal static string ReadFindingCode(LibraryInspectFindingCode value)
        => value switch
        {
            LibraryInspectFindingCode.InvalidId => "library-inspect.invalid-id",
            LibraryInspectFindingCode.UnknownId => "library-inspect.unknown-id",
            LibraryInspectFindingCode.RecordInvalid => "library-inspect.record-invalid",
            LibraryInspectFindingCode.RecordUnavailable => "library-inspect.record-unavailable",
            LibraryInspectFindingCode.RecordBlocked => "library-inspect.record-blocked",
            LibraryInspectFindingCode.SourceRootInvalid => "library-inspect.source-root-invalid",
            LibraryInspectFindingCode.SourceRootUnavailable => "library-inspect.source-root-unavailable",
            LibraryInspectFindingCode.SourceRootBlocked => "library-inspect.source-root-blocked",
            LibraryInspectFindingCode.InventoryIncomplete => "library-inspect.inventory-incomplete",
            LibraryInspectFindingCode.PathAdded => "library-inspect.path-added",
            LibraryInspectFindingCode.PathRetired => "library-inspect.path-retired",
            LibraryInspectFindingCode.LinkMissing => "library-inspect.link-missing",
            LibraryInspectFindingCode.LinkChanged => "library-inspect.link-changed",
            LibraryInspectFindingCode.LinkBlocked => "library-inspect.link-blocked",
            LibraryInspectFindingCode.OperationFailed => "library-inspect.operation-failed",
            LibraryInspectFindingCode.Interrupted => "library-inspect.interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library finding code is not defined."),
        };
}
