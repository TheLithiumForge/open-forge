using OpenForge.Cli.Core.Commands.Library.Detach.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Completion;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Shared.Binding;

internal static class LibraryDetachRequestBinder
{
    internal static CliBindResult<LibraryDetachRequest, LibraryDetachResult> Bind(CliBindingParse parse, CliInvocation invocation, LibraryDetachSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        ArgumentNullException.ThrowIfNull(symbols);
        var supplied = parse.Result.GetValue(symbols.LibraryId);
        if (TryCreateId(supplied) is not { } libraryId)
        {
            return CliBindResult<LibraryDetachRequest, LibraryDetachResult>.Invalid(
                Invalid(invocation.Workspace, supplied, LibraryDetachFindingCode.InvalidId,
                    "Library Detach requires exactly one ID matching [a-z0-9]+(-[a-z0-9]+)* with length 1 through 128."));
        }

        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("A bound Library Detach invocation requires a selected workspace.");
        return CliBindResult<LibraryDetachRequest, LibraryDetachResult>.Bound(new LibraryDetachRequest
        {
            Workspace = workspace,
            LibraryId = libraryId,
            Mode = parse.Result.GetValue(symbols.DryRun) ? LibraryMode.DryRun : LibraryMode.Apply,
        });
    }

    internal static LibraryDetachResult CreateInvalid(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var supplied = ReadAttemptedId(input.BindingParse.OriginalArguments);
        if (input.InvalidInput.Source == CliInvalidInputSource.Workspace)
        {
            var blocked = input.WorkspaceSelectionState is CliWorkspaceSelectionState.NotDirectory
                or CliWorkspaceSelectionState.Unsafe;
            return Invalid(
                workspace: null,
                supplied,
                blocked ? LibraryDetachFindingCode.RecordBlocked : LibraryDetachFindingCode.RecordUnavailable,
                ReadCause(input),
                blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete);
        }

        return Invalid(
            workspace: null,
            supplied,
            LibraryDetachFindingCode.InvalidInput,
            ReadCause(input));
    }

    private static LibraryId? TryCreateId(string? value)
    {
        try
        {
            return LibraryId.Create(value ?? string.Empty);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static LibraryDetachResult Invalid(
        CliWorkspace? workspace,
        string? id,
        LibraryDetachFindingCode code,
        string cause,
        CliSemanticStatus status = CliSemanticStatus.Invalid)
        => new()
        {
            Status = status,
            Workspace = workspace,
            Next = null,
            Result = new LibraryDetachPayload
            {
                Identity = LibraryMutationCompletionProjection.Identity(id, sourceRoot: null, LibraryMode.Apply, sourceIndependent: true),
                Record = LibraryMutationCompletionProjection.Record(read: null, id, intended: null),
                Projection = LibraryMutationCompletionProjection.Projection(
                    record: null, source: null, mappings: null, ownership: null, id,
                    LibraryPlanState.NotStarted, sourceIndependent: true),
                Plan = LibraryMutationCompletionProjection.Plan(
                    LibraryPlanState.NotStarted, directories: null, links: null, generatedRegions: null, recordChange: null),
                Application = LibraryMutationCompletionProjection.NotStarted(),
                Findings =
                [
                    new LibraryDetachFinding
                    {
                        Code = code,
                        Status = status,
                        LibraryId = id,
                        Path = null,
                        Cause = cause,
                    },
                ],
            },
        };

    private static string? ReadAttemptedId(IReadOnlyList<string> arguments)
        => arguments.FirstOrDefault(argument => argument.Length == 0 || argument[0] != '-');

    private static string ReadCause(CliInvalidBindingInput input)
        => input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
}
