using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Binding;

internal static class LibraryInspectRequestBinder
{
    internal static CliBindResult<LibraryInspectRequest, LibraryInspectResult> Bind(CliBindingParse parse, CliInvocation invocation, LibraryInspectSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        ArgumentNullException.ThrowIfNull(symbols);
        var supplied = parse.Result.GetValue(symbols.LibraryId);
        if (TryCreateId(supplied) is not { } libraryId)
        {
            return CliBindResult<LibraryInspectRequest, LibraryInspectResult>.Invalid(
                Invalid(invocation.Workspace, supplied));
        }

        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException(
                "A bound Library Inspect invocation requires a selected workspace.");
        return CliBindResult<LibraryInspectRequest, LibraryInspectResult>.Bound(
            new LibraryInspectRequest
            {
                Workspace = workspace,
                LibraryId = libraryId,
            });
    }

    internal static LibraryInspectResult CreateInvalid(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.InvalidInput.Source == CliInvalidInputSource.Workspace)
        {
            return WorkspaceUnavailable(
                input,
                input.BindingParse.Result.GetValue<string?>(LibraryDefinitions.LibraryId.Name));
        }

        return Invalid(
            workspace: null,
            ReadAttemptedId(input.BindingParse.OriginalArguments));
    }

    private static LibraryId? TryCreateId(string? supplied)
    {
        try
        {
            return LibraryId.Create(supplied ?? string.Empty);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static string? ReadAttemptedId(IReadOnlyList<string> arguments)
        => arguments.FirstOrDefault(argument => argument.Length == 0 || argument[0] != '-');

    private static LibraryInspectResult WorkspaceUnavailable(
        CliInvalidBindingInput input,
        string? supplied)
    {
        var blocked = input.WorkspaceSelectionState is CliWorkspaceSelectionState.NotDirectory
            or CliWorkspaceSelectionState.Unsafe;
        var status = blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete;
        return new LibraryInspectResult
        {
            Status = status,
            Workspace = null,
            Result = new LibraryInspectPayload
            {
                Record = new LibraryInspectRecordView
                {
                    Path = LibraryPathIdentity.RecordRelativePath,
                    State = blocked ? LibraryRecordViewState.Blocked : LibraryRecordViewState.Unavailable,
                    Id = supplied,
                    SourceRoot = null,
                    DestinationRoot = null,
                    RegisteredPaths = [],
                },
                Source = new LibraryInspectSourceView
                {
                    RootState = LibrarySourceRootViewState.NotStarted,
                    State = LibraryInventoryViewState.NotStarted,
                    EligiblePaths = [],
                },
                Projection = new LibraryInspectProjectionView
                {
                    State = blocked ? LibraryCoverage.Blocked : LibraryCoverage.Incomplete,
                    Comparisons = [],
                },
                Findings =
                [
                    new LibraryInspectFinding
                    {
                        Code = blocked
                            ? LibraryInspectFindingCode.RecordBlocked
                            : LibraryInspectFindingCode.RecordUnavailable,
                        Status = status,
                        LibraryId = supplied,
                        Path = LibraryPathIdentity.RecordRelativePath,
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

    private static LibraryInspectResult Invalid(
        Framework.Workspace.CliWorkspace? workspace,
        string? supplied)
        => new()
        {
            Status = CliSemanticStatus.Invalid,
            Workspace = workspace,
            Result = new LibraryInspectPayload
            {
                Record = new LibraryInspectRecordView
                {
                    Path = LibraryPathIdentity.RecordRelativePath,
                    State = LibraryRecordViewState.NotStarted,
                    Id = supplied,
                    SourceRoot = null,
                    DestinationRoot = null,
                    RegisteredPaths = [],
                },
                Source = new LibraryInspectSourceView
                {
                    RootState = LibrarySourceRootViewState.NotStarted,
                    State = LibraryInventoryViewState.NotStarted,
                    EligiblePaths = [],
                },
                Projection = new LibraryInspectProjectionView
                {
                    State = LibraryCoverage.NotStarted,
                    Comparisons = [],
                },
                Findings =
                [
                    new LibraryInspectFinding
                    {
                        Code = LibraryInspectFindingCode.InvalidId,
                        Status = CliSemanticStatus.Invalid,
                        LibraryId = supplied,
                        Path = null,
                        Cause = "Library Inspect requires exactly one ID matching [a-z0-9]+(-[a-z0-9]+)* with length 1 through 128.",
                    },
                ],
            },
        };
}
