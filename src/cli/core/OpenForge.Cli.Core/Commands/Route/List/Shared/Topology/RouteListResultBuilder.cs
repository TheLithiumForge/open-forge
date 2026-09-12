using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListResultBuilder
{
    internal RouteListResult Build(
        RouteListTopologyInput input,
        RouteListTopologySelectionFacts selection,
        RouteListCoverage coverage)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(coverage);
        if (!ReferenceEquals(input.Selection.Selection, selection.Selection)
            || coverage.SelectedRootCount != selection.SelectedRootCount
            || coverage.ConfirmedRowCount != selection.Rows.Count)
        {
            throw new ArgumentException("The route-list result facts must retain one topology selection.", nameof(selection));
        }

        return RouteListResult.Create(
            selection.Status,
            input.Request.Workspace,
            selection.Selection,
            coverage,
            selection.Rows,
            selection.Findings,
            ReadNextAction(selection.Status));
    }

    internal RouteListResult BuildFailure(
        RouteListRequest request,
        RouteListSelection selection)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(selection);
        const string cause = "An unexpected failure prevented route topology formation.";
        var finding = new RouteListFinding(
            RouteListFindingCode.OperationFailed,
            CliSemanticStatus.Failed,
            selection.AttemptedId ?? selection.AttemptedPath,
            cause);
        var coverage = RouteListCoverage.Failed(
            request.RequestedDepth,
            null,
            selectedRootCount: 0,
            confirmedRowCount: 0,
            [],
            [cause]);
        return RouteListResult.Create(
            CliSemanticStatus.Failed,
            request.Workspace,
            selection,
            coverage,
            [],
            [finding],
            ReadNextAction(CliSemanticStatus.Failed));
    }

    internal static CliNextAction? ReadNextAction(CliSemanticStatus status)
    {
        return status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => null,
            CliSemanticStatus.Invalid => new CliNextAction(
                "open-forge route list --help",
                "Review the route-list source and depth input."),
            CliSemanticStatus.Incomplete => new CliNextAction(
                "open-forge route list",
                "Review the unresolved route boundary, then rerun the operation."),
            CliSemanticStatus.Blocked => new CliNextAction(
                "open-forge route list",
                "Resolve the blocked route boundary, then rerun the operation."),
            CliSemanticStatus.Failed => new CliNextAction(
                "open-forge route list",
                "Address the reported failure, then rerun the operation."),
            CliSemanticStatus.Interrupted => new CliNextAction(
                "open-forge route list",
                "Rerun the operation when route enumeration can continue."),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The route-list status is not defined."),
        };
    }
}
