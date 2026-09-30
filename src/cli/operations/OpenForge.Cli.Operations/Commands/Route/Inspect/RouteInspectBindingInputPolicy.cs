using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Inspect;

internal static class RouteInspectBindingInputPolicy
{
    internal static RouteInspectResult CreateSourceCardinalityInvalidResult(
        CliInvocation invocation,
        IReadOnlyList<string> sourceReferences,
        bool matchingFiles)
    {
        ArgumentNullException.ThrowIfNull(invocation);
        ArgumentNullException.ThrowIfNull(sourceReferences);
        var missing = sourceReferences.Count == 0;
        RouteInspectSelection selection;
        string subject;
        if (missing)
        {
            selection = new RouteInspectSelection(
                RouteInspectReferenceKind.Missing,
                RouteInspectSelectionMethod.Unresolved,
                null,
                []);
            subject = "source-reference";
        }
        else
        {
            var requestedReference = string.Join(" ", sourceReferences);
            var parsed = SourceReferenceParser.Parse(requestedReference);
            selection = RouteInspectResolutionSupport.UnresolvedSelection(parsed);
            subject = requestedReference;
        }

        var code = missing
            ? RouteInspectConditionCode.MissingSource
            : RouteInspectConditionCode.MultipleSources;
        var message = missing
            ? "route inspect requires one source reference."
            : "route inspect accepts exactly one source reference.";
        return CreateInvalidResult(
            invocation.Workspace,
            selection,
            new RouteInspectCondition(
                code,
                CliSemanticStatus.Invalid,
                subject,
                message),
            matchingFiles);
    }

    internal static RouteInspectResult CreateWorkspaceInvalidResult(
        CliInvalidBindingInput input,
        RouteInspectSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(symbols);
        var sourceReferences = input.BindingParse.Result.GetValue(symbols.SourceReferences) ?? [];
        var selection = sourceReferences.Length switch
        {
            0 => new RouteInspectSelection(
                RouteInspectReferenceKind.Missing,
                RouteInspectSelectionMethod.Unresolved,
                null,
                []),
            1 => RouteInspectResolutionSupport.UnresolvedSelection(
                SourceReferenceParser.Parse(sourceReferences[0])),
            _ => RouteInspectResolutionSupport.UnresolvedSelection(
                SourceReferenceParser.Parse(string.Join(" ", sourceReferences))),
        };
        var subject = input.GlobalInput.WorkspaceValue
            ?? input.ProcessEnvironment.CurrentDirectory;
        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        return CreateInvalidResult(
            null,
            selection,
            new RouteInspectCondition(
                RouteInspectConditionCode.InvalidWorkspace,
                CliSemanticStatus.Invalid,
                subject,
                cause),
            input.BindingParse.Result.GetValue(symbols.MatchingFiles));
    }

    private static RouteInspectResult CreateInvalidResult(
        CliWorkspace? workspace,
        RouteInspectSelection selection,
        RouteInspectCondition condition,
        bool matchingFiles)
    {
        return RouteInspectResult.Create(
            CliSemanticStatus.Invalid,
            workspace,
            selection,
            null,
            null,
            [],
            [condition],
            new CliNextAction(
                "open-forge route inspect --help",
                "Correct the named source or input, then rerun route inspect."),
            matchingFiles: matchingFiles
                ? RouteInspectMatchingFiles.Unavailable(null, RouteInspectMatchingFilesReason.SourceUnavailable)
                : null);
    }

    internal static RouteInspectResult CreateWorkingPathsInvalidResult(
        CliWorkspace workspace,
        string sourceReference,
        bool matchingFiles)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceReference);
        return CreateInvalidResult(
            workspace,
            RouteInspectResolutionSupport.UnresolvedSelection(SourceReferenceParser.Parse(sourceReference)),
            new RouteInspectCondition(
                RouteInspectConditionCode.InvalidWorkingPath,
                CliSemanticStatus.Invalid,
                "--for",
                "Each --for path must resolve inside the selected workspace."),
            matchingFiles);
    }
}
