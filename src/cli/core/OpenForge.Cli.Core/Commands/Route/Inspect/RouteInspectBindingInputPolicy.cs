using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;

namespace OpenForge.Cli.Core.Commands.Route.Inspect;

internal static class RouteInspectBindingInputPolicy
{
    internal static RouteInspectResult CreateSourceCardinalityInvalidResult(
        CliInvocation invocation,
        IReadOnlyList<string> sourceReferences)
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
                message));
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
                cause));
    }

    private static RouteInspectResult CreateInvalidResult(
        CliWorkspace? workspace,
        RouteInspectSelection selection,
        RouteInspectCondition condition)
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
                "Correct the named source or input, then rerun route inspect."));
    }
}
