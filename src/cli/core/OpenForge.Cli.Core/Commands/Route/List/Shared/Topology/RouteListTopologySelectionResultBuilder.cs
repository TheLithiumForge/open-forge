using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologySelectionResultBuilder
{
    private readonly RouteListTopologyInput _input;
    private readonly RouteListTopologySelectionState _state;

    internal RouteListTopologySelectionResultBuilder(
        RouteListTopologyInput input,
        RouteListTopologySelectionState state)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(state);
        _input = input;
        _state = state;
    }

    internal RouteListTopologySelectionFacts FormFacts(int selectedRootCount)
    {
        var orderedFindings = RouteListTopologyFindingPolicy.OrderDistinct(_state.Findings);
        var status = RouteListTopologyFindingPolicy.ReadAggregateStatus(
            _input.Selection.State,
            orderedFindings);
        selectedRootCount = status == CliSemanticStatus.Invalid
            ? 0
            : selectedRootCount;
        var statusRule = RouteListDefinitions.ReadFindingResultStatusRule(status);
        RouteListFinding[] findings = statusRule.RequiredFindingStatus is null
            ? []
            : orderedFindings
                .Where(finding => statusRule.AllowedFindingStatuses.Contains(finding.Status))
                .ToArray();
        var rows = status == CliSemanticStatus.Invalid
            ? []
            : _state.Rows.ToArray();
        var effectiveDepth = ReadEffectiveDepth(status);
        string[] evidence = status is CliSemanticStatus.Complete or CliSemanticStatus.Attention
            ? ["The selected route roots and requested structural depth were confirmed."]
            : [$"{rows.Length} safe route rows were confirmed."];
        var unresolvedBoundaries = findings
            .Where(finding => finding.Status != CliSemanticStatus.Attention)
            .Select(RouteListTopologyFindingPolicy.ReadBoundary)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return new RouteListTopologySelectionFacts(
            status,
            _input.Selection.Selection,
            selectedRootCount,
            effectiveDepth,
            rows,
            findings,
            evidence,
            unresolvedBoundaries);
    }

    private RouteListDepth? ReadEffectiveDepth(CliSemanticStatus status)
    {
        if (status is CliSemanticStatus.Complete or CliSemanticStatus.Attention)
        {
            return _input.Request.RequestedDepth;
        }

        if (status is CliSemanticStatus.Invalid or CliSemanticStatus.Failed
            || _state.FirstUnresolvedDepth is null or <= 0)
        {
            return null;
        }

        var completeDepth = _state.FirstUnresolvedDepth.Value - 1;
        var requestedDepth = _input.Request.RequestedDepth;
        if (requestedDepth.Kind == RouteListDepthKind.Finite)
        {
            completeDepth = Math.Min(completeDepth, requestedDepth.FiniteValue);
        }

        return RouteListDepth.Finite(completeDepth);
    }
}
