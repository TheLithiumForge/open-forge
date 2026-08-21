using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologySelectionFacts
{
    internal RouteListTopologySelectionFacts(
        CliSemanticStatus status,
        RouteListSelection selection,
        int selectedRootCount,
        RouteListDepth? effectiveDepth,
        IEnumerable<RouteListRow> rows,
        IEnumerable<RouteListFinding> findings,
        IEnumerable<string> evidence,
        IEnumerable<string> unresolvedBoundaries)
    {
        _ = CliStatusDefinitions.Read(status);
        ArgumentNullException.ThrowIfNull(selection);
        if (selectedRootCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(selectedRootCount), selectedRootCount, "Selected root count cannot be negative.");
        }

        var materializedRows = Materialize(rows, nameof(rows));
        var materializedFindings = Materialize(findings, nameof(findings));
        var materializedEvidence = MaterializeText(evidence, nameof(evidence));
        var materializedBoundaries = MaterializeText(unresolvedBoundaries, nameof(unresolvedBoundaries));
        var rootCount = materializedRows.Count(row => row.RelativeDepth == 0);
        if (rootCount > selectedRootCount)
        {
            throw new ArgumentException("Topology rows cannot exceed the selected root count.", nameof(rows));
        }

        if (status is CliSemanticStatus.Complete or CliSemanticStatus.Attention)
        {
            if (!selection.IsResolved || rootCount != selectedRootCount || effectiveDepth is null)
            {
                throw new ArgumentException("Complete topology selection requires every selected root and an effective depth.");
            }

            if (materializedBoundaries.Count != 0)
            {
                throw new ArgumentException("Complete topology selection cannot contain an unresolved boundary.", nameof(unresolvedBoundaries));
            }
        }
        else if (status == CliSemanticStatus.Invalid)
        {
            if (selectedRootCount != 0 || materializedRows.Count != 0 || effectiveDepth is not null)
            {
                throw new ArgumentException("Invalid topology selection cannot retain roots, rows, or an effective depth.");
            }
        }
        else if (materializedBoundaries.Count == 0)
        {
            throw new ArgumentException("Non-complete topology selection requires an unresolved boundary.", nameof(unresolvedBoundaries));
        }

        ValidateFindingSet(status, materializedFindings);
        Status = status;
        Selection = selection;
        SelectedRootCount = selectedRootCount;
        EffectiveDepth = effectiveDepth;
        Rows = materializedRows;
        Findings = materializedFindings;
        Evidence = materializedEvidence;
        UnresolvedBoundaries = materializedBoundaries;
    }

    internal CliSemanticStatus Status { get; }

    internal RouteListSelection Selection { get; }

    internal int SelectedRootCount { get; }

    internal RouteListDepth? EffectiveDepth { get; }

    internal IReadOnlyList<RouteListRow> Rows { get; }

    internal IReadOnlyList<RouteListFinding> Findings { get; }

    internal IReadOnlyList<string> Evidence { get; }

    internal IReadOnlyList<string> UnresolvedBoundaries { get; }

    private static IReadOnlyList<T> Materialize<T>(IEnumerable<T> values, string parameterName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var materialized = values.ToArray();
        if (materialized.Any(value => value is null))
        {
            throw new ArgumentException("Topology facts cannot contain null.", parameterName);
        }

        return new ReadOnlyCollection<T>(materialized);
    }

    private static IReadOnlyList<string> MaterializeText(
        IEnumerable<string> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var materialized = values.ToArray();
        if (materialized.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Topology evidence cannot contain an empty value.", parameterName);
        }

        return new ReadOnlyCollection<string>(materialized);
    }

    private static void ValidateFindingSet(
        CliSemanticStatus status,
        IReadOnlyList<RouteListFinding> findings)
    {
        var rule = RouteListDefinitions.ReadFindingResultStatusRule(status);
        if (rule.RequiredFindingStatus is null)
        {
            if (findings.Count != 0)
            {
                throw new ArgumentException("Complete topology selection cannot contain findings.", nameof(findings));
            }

            return;
        }

        if (!findings.Any(finding => finding.Status == rule.RequiredFindingStatus.Value)
            || findings.Any(finding => !rule.AllowedFindingStatuses.Contains(finding.Status)))
        {
            throw new ArgumentException("Topology findings do not match the aggregate status.", nameof(findings));
        }
    }
}
