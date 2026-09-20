using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Completion;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Shared.Binding;

internal static class LibrarySyncRequestBinder
{
    internal static CliBindResult<LibrarySyncRequest, LibrarySyncResult> Bind(CliBindingParse parse, CliInvocation invocation, LibrarySyncSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        ArgumentNullException.ThrowIfNull(symbols);
        var supplied = parse.Result.GetValue(symbols.LibraryId);
        if (LibraryId.TryCreate(supplied) is not { } libraryId)
        {
            return CliBindResult<LibrarySyncRequest, LibrarySyncResult>.Invalid(
                Invalid(invocation.Workspace, supplied, LibrarySyncFindingCode.InvalidId,
                    "Library Sync requires exactly one ID matching [a-z0-9]+(-[a-z0-9]+)* with length 1 through 128."));
        }

        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("A bound Library Sync invocation requires a selected workspace.");
        var automatic = parse.Result.GetValue(symbols.Automatic);
        var dryRun = parse.Result.GetValue(symbols.DryRun);
        return CliBindResult<LibrarySyncRequest, LibrarySyncResult>.Bound(new LibrarySyncRequest
        {
            AllowPrompt = invocation.Presentation.Format == CliFormat.Text && !dryRun && !automatic,
            Automatic = automatic,
            Workspace = workspace,
            LibraryId = libraryId,
            Mode = dryRun ? LibraryMode.DryRun : LibraryMode.Apply,
            Allow = [.. CliOptionResultFactsReader.ReadValues(parse.Result, symbols.Allow)],
        });
    }

    internal static LibrarySyncResult CreateInvalid(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.InvalidInput.Source == CliInvalidInputSource.Workspace)
        {
            var blocked = input.WorkspaceSelectionState is CliWorkspaceSelectionState.NotDirectory
                or CliWorkspaceSelectionState.Unsafe;
            return Invalid(
                workspace: null,
                input.BindingParse.Result.GetValue<string?>(LibraryDefinitions.LibraryId.Name),
                blocked ? LibrarySyncFindingCode.RecordBlocked : LibrarySyncFindingCode.RecordUnavailable,
                ReadCause(input),
                blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete);
        }

        return Invalid(
            workspace: null,
            ReadAttemptedId(input.BindingParse.OriginalArguments),
            LibrarySyncFindingCode.InvalidInput,
            ReadCause(input));
    }


    private static LibrarySyncResult Invalid(
        CliWorkspace? workspace,
        string? id,
        LibrarySyncFindingCode code,
        string cause,
        CliSemanticStatus status = CliSemanticStatus.Invalid)
        => new()
        {
            Status = status,
            Workspace = workspace,
            Next = null,
            Result = new LibrarySyncPayload
            {
                Permissions = LibraryPermissionView.NotEvaluated(),
                Identity = LibraryMutationCompletionProjection.Identity(id, sourceRoot: null, destinationRoot: null, LibraryMode.Apply, sourceIndependent: false),
                Record = LibraryMutationCompletionProjection.Record(read: null, id, intended: null),
                Source = LibraryMutationCompletionProjection.Source(read: null, destinationRoot: null),
                Projection = LibraryMutationCompletionProjection.Projection(
                    observation: null, id,
                    LibraryPlanState.NotStarted, sourceIndependent: false),
                Plan = LibraryMutationCompletionProjection.NotPlanned(),
                Application = LibraryMutationCompletionProjection.NotStarted(),
                Findings =
                [
                    new LibrarySyncFinding
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
