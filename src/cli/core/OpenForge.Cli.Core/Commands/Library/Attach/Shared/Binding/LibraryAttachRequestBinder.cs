using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
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

namespace OpenForge.Cli.Core.Commands.Library.Attach.Shared.Binding;

internal static class LibraryAttachRequestBinder
{
    internal static CliBindResult<LibraryAttachRequest, LibraryAttachResult> Bind(CliBindingParse parse, CliInvocation invocation, LibraryAttachSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        ArgumentNullException.ThrowIfNull(symbols);
        var suppliedId = parse.Result.GetValue(symbols.LibraryId);
        var suppliedRoot = parse.Result.GetValue(symbols.SourceRoot);
        if (TryCreateId(suppliedId) is not { } libraryId)
        {
            return CliBindResult<LibraryAttachRequest, LibraryAttachResult>.Invalid(
                Invalid(invocation.Workspace, suppliedId, suppliedRoot, LibraryAttachFindingCode.InvalidId));
        }

        if (TryCreateRoot(suppliedRoot) is not { } sourceRoot)
        {
            return CliBindResult<LibraryAttachRequest, LibraryAttachResult>.Invalid(
                Invalid(invocation.Workspace, suppliedId, suppliedRoot, LibraryAttachFindingCode.SourceRootInvalid));
        }

        LibraryDestinationRoot destinationRoot;
        try
        {
            destinationRoot = LibraryDestinationRoot.Create(
                parse.Result.GetValue(symbols.DestinationRoot) ?? LibraryDestinationRoot.WorkspaceRootValue);
        }
        catch (ArgumentException)
        {
            return CliBindResult<LibraryAttachRequest, LibraryAttachResult>.Invalid(
                Invalid(invocation.Workspace, suppliedId, suppliedRoot, LibraryAttachFindingCode.DestinationRootInvalid));
        }

        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("A bound Library Attach invocation requires a selected workspace.");
        return CliBindResult<LibraryAttachRequest, LibraryAttachResult>.Bound(new LibraryAttachRequest
        {
            AllowPrompt = invocation.Presentation.Format == CliOutputFormat.Human && !parse.Result.GetValue(symbols.DryRun),
            DestinationRoot = destinationRoot,
            Workspace = workspace,
            LibraryId = libraryId,
            SourceRoot = sourceRoot,
            Mode = parse.Result.GetValue(symbols.DryRun) ? LibraryMode.DryRun : LibraryMode.Apply,
        });
    }

    internal static LibraryAttachResult CreateInvalid(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var supplied = ReadOperands(input.BindingParse.OriginalArguments);
        if (input.InvalidInput.Source == CliInvalidInputSource.Workspace)
        {
            var blocked = input.WorkspaceSelectionState is CliWorkspaceSelectionState.NotDirectory
                or CliWorkspaceSelectionState.Unsafe;
            var status = blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete;
            return Create(
                status,
                workspace: null,
                supplied.ElementAtOrDefault(0),
                supplied.ElementAtOrDefault(1),
                blocked ? LibraryAttachFindingCode.RecordBlocked : LibraryAttachFindingCode.RecordUnavailable,
                ReadCause(input));
        }

        return Create(
            CliSemanticStatus.Invalid,
            workspace: null,
            supplied.ElementAtOrDefault(0),
            supplied.ElementAtOrDefault(1),
            LibraryAttachFindingCode.InvalidInput,
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

    private static WorkspaceRelativeDirectory? TryCreateRoot(string? value)
    {
        try
        {
            return WorkspaceRelativeDirectory.Create(value ?? string.Empty);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static LibraryAttachResult Invalid(
        CliWorkspace? workspace,
        string? id,
        string? sourceRoot,
        LibraryAttachFindingCode code)
        => Create(
            CliSemanticStatus.Invalid,
            workspace,
            id,
            sourceRoot,
            code,
            code switch
            {
                LibraryAttachFindingCode.InvalidId => "Library Attach requires exactly one ID matching [a-z0-9]+(-[a-z0-9]+)* with length 1 through 128.",
                LibraryAttachFindingCode.SourceRootInvalid => "Library Attach requires one portable workspace-relative source root.",
                LibraryAttachFindingCode.DestinationRootInvalid => "Library Attach requires one portable workspace-relative destination directory or dot for the workspace root.",
                _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Library Attach binding failure is not defined."),
            });

    private static LibraryAttachResult Create(
        CliSemanticStatus status,
        CliWorkspace? workspace,
        string? id,
        string? sourceRoot,
        LibraryAttachFindingCode code,
        string cause)
        => new()
        {
            Status = status,
            Workspace = workspace,
            Next = null,
            Result = new LibraryAttachPayload
            {
                Permissions = LibraryPermissionView.NotEvaluated(),
                Identity = LibraryMutationCompletionProjection.Identity(id, sourceRoot, destinationRoot: null, LibraryMode.Apply, sourceIndependent: false),
                Record = LibraryMutationCompletionProjection.Record(read: null, id, intended: null),
                Source = LibraryMutationCompletionProjection.Source(read: null, destinationRoot: null),
                Projection = LibraryMutationCompletionProjection.Projection(
                    record: null, source: null, mappings: null, ownership: null, id,
                    LibraryPlanState.NotStarted, sourceIndependent: false),
                Plan = LibraryMutationCompletionProjection.NotPlanned(),
                Application = LibraryMutationCompletionProjection.NotStarted(),
                Findings =
                [
                    new LibraryAttachFinding
                    {
                        Code = code,
                        Status = status,
                        LibraryId = id,
                        Path = sourceRoot,
                        Cause = cause,
                    },
                ],
            },
        };

    private static string[] ReadOperands(IReadOnlyList<string> arguments)
        => [.. arguments.Where(argument => argument.Length == 0 || argument[0] != '-')];

    private static string ReadCause(CliInvalidBindingInput input)
        => input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
}
