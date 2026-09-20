using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Commands.Library.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.List.Models.Request;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Library.List.Shared.Binding;

internal static class LibraryListRequestBinder
{
    internal static CliBindResult<LibraryListRequest, LibraryListResult> Bind(CliBindingParse parse, CliInvocation invocation, LibraryListSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        ArgumentNullException.ThrowIfNull(symbols);
        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException(
                "A bound Library List invocation requires a selected workspace.");
        return CliBindResult<LibraryListRequest, LibraryListResult>.Bound(
            new LibraryListRequest { Workspace = workspace });
    }

    internal static LibraryListResult CreateInvalid(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.InvalidInput.Source == CliInvalidInputSource.Workspace)
        {
            var blocked = input.WorkspaceSelectionState is CliWorkspaceSelectionState.NotDirectory
                or CliWorkspaceSelectionState.Unsafe;
            var status = blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete;
            return new LibraryListResult
            {
                Status = status,
                Workspace = null,
                Result = new LibraryListPayload
                {
                    Record = new LibraryListRecordView
                    {
                        Path = WorkspaceOwnershipDefinitions.RelativePath,
                        State = blocked ? LibraryRecordViewState.Blocked : LibraryRecordViewState.Unavailable,
                        LibraryCount = null,
                    },
                    Libraries = [],
                    Inventory = LibraryListInventoryState.NotStarted,
                    Coverage = blocked ? LibraryCoverage.Blocked : LibraryCoverage.Incomplete,
                    Findings =
                    [
                        new LibraryListFinding
                        {
                            Code = blocked
                                ? LibraryListFindingCode.RecordBlocked
                                : LibraryListFindingCode.RecordUnavailable,
                            Status = status,
                            LibraryId = null,
                            Path = WorkspaceOwnershipDefinitions.RelativePath,
                            Cause = ReadCause(input),
                        },
                    ],
                },
            };
        }

        return new LibraryListResult
        {
            Status = CliSemanticStatus.Invalid,
            Workspace = null,
            Result = new LibraryListPayload
            {
                Record = new LibraryListRecordView
                {
                    Path = WorkspaceOwnershipDefinitions.RelativePath,
                    State = LibraryRecordViewState.NotStarted,
                    LibraryCount = null,
                },
                Libraries = [],
                Inventory = LibraryListInventoryState.NotStarted,
                Coverage = LibraryCoverage.NotStarted,
                Findings =
                [
                    new LibraryListFinding
                    {
                        Code = LibraryListFindingCode.InvalidInput,
                        Status = CliSemanticStatus.Invalid,
                        LibraryId = null,
                        Path = null,
                        Cause = ReadCause(input),
                    },
                ],
            },
        };
    }

    private static string ReadCause(CliInvalidBindingInput input)
        => input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
}
