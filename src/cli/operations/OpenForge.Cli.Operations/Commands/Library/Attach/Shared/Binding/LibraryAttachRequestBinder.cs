using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Binding;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Completion;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

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
        if (LibraryId.TryCreate(suppliedId) is not { } libraryId)
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
        var suppliedDestination = parse.Result.GetValue(symbols.DestinationRoot);
        try
        {
            destinationRoot = LibraryDestinationRoot.Create(
                suppliedDestination ?? LibraryDestinationRoot.WorkspaceRootValue);
        }
        catch (ArgumentException)
        {
            return CliBindResult<LibraryAttachRequest, LibraryAttachResult>.Invalid(
                Invalid(invocation.Workspace, suppliedId, suppliedRoot, LibraryAttachFindingCode.DestinationRootInvalid, suppliedDestination));
        }

        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("A bound Library Attach invocation requires a selected workspace.");
        var automatic = parse.Result.GetValue(symbols.Automatic);
        var dryRun = parse.Result.GetValue(symbols.DryRun);
        return CliBindResult<LibraryAttachRequest, LibraryAttachResult>.Bound(new LibraryAttachRequest
        {
            AllowPrompt = invocation.Presentation.Format == CliFormat.Text && !dryRun && !automatic,
            Automatic = automatic,
            DestinationRoot = destinationRoot,
            Workspace = workspace,
            LibraryId = libraryId,
            SourceRoot = sourceRoot,
            Mode = dryRun ? LibraryMode.DryRun : LibraryMode.Apply,
            Allow = [.. CliOptionResultFactsReader.ReadValues(parse.Result, symbols.Allow)],
        });
    }

    internal static LibraryAttachResult CreateInvalid(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.InvalidInput.Source == CliInvalidInputSource.Workspace)
        {
            var blocked = input.WorkspaceSelectionState is CliWorkspaceSelectionState.NotDirectory
                or CliWorkspaceSelectionState.Unsafe;
            var status = blocked ? CliSemanticStatus.Blocked : CliSemanticStatus.Incomplete;
            return Create(
                status,
                workspace: null,
                input.BindingParse.Result.GetValue<string?>(LibraryDefinitions.LibraryId.Name),
                input.BindingParse.Result.GetValue<string?>(LibraryDefinitions.SourceRoot.Name),
                blocked ? LibraryAttachFindingCode.RecordBlocked : LibraryAttachFindingCode.RecordUnavailable,
                null,
                ReadCause(input));
        }

        var supplied = ReadOperands(input.BindingParse.OriginalArguments);
        return Create(
            CliSemanticStatus.Invalid,
            workspace: null,
            supplied.ElementAtOrDefault(0),
            supplied.ElementAtOrDefault(1),
            LibraryAttachFindingCode.InvalidInput,
            null,
            ReadCause(input));
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
        LibraryAttachFindingCode code,
        string? findingPath = null)
        => Create(
            CliSemanticStatus.Invalid,
            workspace,
            id,
            sourceRoot,
            code,
            findingPath ?? sourceRoot,
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
        string? findingPath,
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
                    observation: null, id,
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
                        Path = findingPath,
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
