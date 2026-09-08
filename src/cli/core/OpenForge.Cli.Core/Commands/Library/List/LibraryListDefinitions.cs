using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.List;

internal static class LibraryListDefinitions
{
    internal const string CommandIdentity = "library list";
    internal static readonly CliSyntaxDefinition Command = new("list", "List bounded Library record and link observations.");
    internal static string ReadFindingCode(LibraryListFindingCode value)
        => value switch
        {
            LibraryListFindingCode.InvalidRecord => "library-list.invalid-record",
            LibraryListFindingCode.RecordUnavailable => "library-list.record-unavailable",
            LibraryListFindingCode.RecordBlocked => "library-list.record-blocked",
            LibraryListFindingCode.SourceRootInvalid => "library-list.source-root-invalid",
            LibraryListFindingCode.SourceRootUnavailable => "library-list.source-root-unavailable",
            LibraryListFindingCode.SourceRootBlocked => "library-list.source-root-blocked",
            LibraryListFindingCode.LinkMissing => "library-list.link-missing",
            LibraryListFindingCode.LinkChanged => "library-list.link-changed",
            LibraryListFindingCode.LinkUnavailable => "library-list.link-unavailable",
            LibraryListFindingCode.LinkBlocked => "library-list.link-blocked",
            LibraryListFindingCode.OperationFailed => "library-list.operation-failed",
            LibraryListFindingCode.Interrupted => "library-list.interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Library finding code is not defined."),
        };
}
