using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListCoverageBuilder
{
    internal RouteListCoverage Build(
        RouteListTopologyInput input,
        RouteListTopologySelectionFacts selection)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(selection);
        if (!ReferenceEquals(input.Selection.Selection, selection.Selection))
        {
            throw new ArgumentException("Coverage must retain the topology input selection.", nameof(selection));
        }

        var requestedDepth = input.Request.RequestedDepth;
        if (selection.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention)
        {
            if (selection.EffectiveDepth is not { } effectiveDepth)
            {
                throw new InvalidOperationException(
                    "Complete route-list topology selection requires an effective depth.");
            }

            return RouteListCoverage.Complete(
                requestedDepth,
                effectiveDepth,
                selection.SelectedRootCount,
                selection.Rows.Count,
                selection.Evidence);
        }

        return selection.Status switch
        {
            CliSemanticStatus.Incomplete => RouteListCoverage.Incomplete(
                requestedDepth,
                selection.EffectiveDepth,
                selection.SelectedRootCount,
                selection.Rows.Count,
                selection.Evidence,
                selection.UnresolvedBoundaries),
            CliSemanticStatus.Invalid => RouteListCoverage.NotStarted(requestedDepth),
            CliSemanticStatus.Blocked => RouteListCoverage.Blocked(
                requestedDepth,
                selection.EffectiveDepth,
                selection.SelectedRootCount,
                selection.Rows.Count,
                selection.Evidence,
                selection.UnresolvedBoundaries),
            CliSemanticStatus.Failed => RouteListCoverage.Failed(
                requestedDepth,
                selection.EffectiveDepth,
                selection.SelectedRootCount,
                selection.Rows.Count,
                selection.Evidence,
                selection.UnresolvedBoundaries),
            CliSemanticStatus.Interrupted => RouteListCoverage.Interrupted(
                requestedDepth,
                selection.EffectiveDepth,
                selection.SelectedRootCount,
                selection.Rows.Count,
                selection.Evidence,
                selection.UnresolvedBoundaries),
            _ => throw new ArgumentOutOfRangeException(
                nameof(selection),
                selection.Status,
                "The route-list topology status is not defined."),
        };
    }
}
