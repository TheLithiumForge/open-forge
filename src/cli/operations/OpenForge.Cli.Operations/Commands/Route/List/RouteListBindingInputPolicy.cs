using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.List;

internal static class RouteListBindingInputPolicy
{
    internal static RouteListResult CreateInvalidDepthResult(
        CliWorkspace? workspace,
        string? sourceReference,
        string? depthSpelling)
    {
        var selection = RouteListSelectionFactory.Attempted(sourceReference);
        var subject = string.IsNullOrWhiteSpace(depthSpelling)
            ? "--depth"
            : depthSpelling;
        var finding = new RouteListFinding(
            RouteListFindingCode.InvalidDepth,
            CliSemanticStatus.Invalid,
            subject,
            "Depth must be a non-negative Int32 or the exact value all.");
        return RouteListResult.Create(
            CliSemanticStatus.Invalid,
            workspace,
            selection,
            RouteListCoverage.NotStarted(null),
            [],
            [finding],
            new CliNextAction(
                "open-forge route list --help",
                "Use a non-negative Int32 depth or all, then rerun the operation."));
    }

    internal static RouteListResult CreateWorkspaceInvalidResult(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        return RouteListResult.Create(
            CliSemanticStatus.Invalid,
            null,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.NotStarted(null),
            [],
            [new RouteListFinding(
                RouteListFindingCode.InvalidWorkspace,
                CliSemanticStatus.Invalid,
                input.GlobalInput.WorkspaceValue ?? input.ProcessEnvironment.CurrentDirectory,
                cause)],
            new CliNextAction(
                "open-forge route list --help",
                "Select an available directory with --workspace, then rerun the operation."));
    }

}
