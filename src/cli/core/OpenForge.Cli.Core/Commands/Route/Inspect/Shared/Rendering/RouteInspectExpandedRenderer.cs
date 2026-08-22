using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectExpandedRenderer
{
    internal static string Render(RouteInspectResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var lines = new List<string>();
        RouteInspectCompactRenderer.Add(lines, result, includeMessages: false);
        lines.Add(string.Empty);
        AddExplanations(lines, result);
        AddMessages(lines, result);
        AddAxioms(lines, result.Profile);
        AddEvidence(lines, result);
        return string.Join(Environment.NewLine, lines);
    }

    private static void AddExplanations(
        ICollection<string> lines,
        RouteInspectResult result)
    {
        lines.Add("Explanations:");
        lines.Add($"  Why: {StatusReason(result.Status)}");
        if (result.Profile?.Reading.Automatic is { State: RouteInspectFactState.Value } automatic)
        {
            foreach (var reason in automatic.Value!.Reasons)
            {
                lines.Add($"  Why: the source is read when {RouteInspectHumanAutomaticReading.Explanation(reason)}");
            }
        }
    }

    private static void AddMessages(
        ICollection<string> lines,
        RouteInspectResult result)
    {
        lines.Add("Observations:");
        if (result.Observations.Count == 0)
        {
            lines.Add("  none");
        }
        else
        {
            foreach (var observation in result.Observations)
            {
                lines.Add($"  - {RouteInspectHumanValues.Text(observation.Message)}");
                AddPaths(lines, observation.Paths);
            }
        }

        lines.Add("Conditions:");
        if (result.Conditions.Count == 0)
        {
            lines.Add("  none");
        }
        else
        {
            foreach (var condition in result.Conditions)
            {
                lines.Add($"  - {RouteInspectHumanValues.Text(condition.Message)}");
                AddPaths(lines, condition.Paths);
            }
        }
    }

    private static void AddAxioms(
        ICollection<string> lines,
        RouteInspectProfile? profile)
    {
        lines.Add("Axioms:");
        if (profile is null)
        {
            lines.Add("  not established");
            return;
        }

        var axioms = profile.Axioms;
        if (axioms.State != RouteInspectFactState.Value)
        {
            lines.Add($"  {RouteInspectHumanValues.FactState(axioms.State)}: {RouteInspectHumanValues.Text(axioms.Reason!)}");
            return;
        }

        var value = axioms.Value!;
        lines.Add($"  Inherited from: {Inherited(value.Inherited)}");
        lines.Add($"  Local: {Local(value.Local)}");
    }

    private static void AddEvidence(
        ICollection<string> lines,
        RouteInspectResult result)
    {
        lines.Add("Evidence:");
        lines.Add($"  Selection: {RouteInspectHumanValues.ReferenceKind(result.Selection.ReferenceKind)} via "
            + RouteInspectHumanValues.SelectionMethod(result.Selection.SelectionMethod));
        if (result.Identity is { } identity)
        {
            lines.Add($"  Physical layers: {identity.PhysicalLayers.Count}");
            foreach (var layer in identity.PhysicalLayers)
            {
                lines.Add($"  - {RouteInspectHumanValues.Text(layer.WorkspaceRelativePath)}");
            }
        }
        else
        {
            lines.Add("  Physical layers: not established");
        }

        if (result.Profile is { } profile)
        {
            lines.Add("  Measurements:");
            lines.Add($"    own source: {RouteInspectHumanMeasurements.Measurement(profile.Measurements.OwnSource)}");
            lines.Add($"    selected closure: {RouteInspectHumanMeasurements.Measurement(profile.Measurements.SelectedClosure)}");
            lines.Add($"    task-start overlap: {RouteInspectHumanMeasurements.Measurement(profile.Measurements.TaskStartOverlap)}");
            lines.Add($"    selection addition: {RouteInspectHumanMeasurements.Measurement(profile.Measurements.SelectionAddition)}");
            lines.Add($"    #LoadNow descendants: {RouteInspectHumanMeasurements.Measurement(profile.Measurements.LoadNowDescendants)}");
        }
    }

    private static void AddPaths(
        ICollection<string> lines,
        IReadOnlyList<string> paths)
    {
        if (paths.Count != 0)
        {
            lines.Add($"    Paths: {string.Join(", ", paths.Select(RouteInspectHumanValues.Text))}");
        }
    }

    private static string StatusReason(CliSemanticStatus status)
    {
        return status switch
        {
            CliSemanticStatus.Complete => "all applicable route facts are available",
            CliSemanticStatus.Attention => "the source is safe and complete but its automatic ID is not unique",
            CliSemanticStatus.Incomplete => "a required route fact or measurement is unavailable",
            CliSemanticStatus.Invalid => "the source reference was not accepted",
            CliSemanticStatus.Blocked => "a safe source or route boundary could not be established",
            CliSemanticStatus.Failed => "an unexpected failure stopped route inspection",
            CliSemanticStatus.Interrupted => "route inspection was interrupted before completion",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The status is not defined."),
        };
    }

    private static string Inherited(RouteInspectFact<RouteInspectAxiomsSources> fact)
    {
        if (fact.State == RouteInspectFactState.Value)
        {
            return fact.Value!.SourceIds.Count == 0
                ? "none"
                : string.Join(", ", fact.Value.SourceIds.Select(RouteInspectHumanValues.Text));
        }

        return $"{RouteInspectHumanValues.FactState(fact.State)} ({RouteInspectHumanValues.Text(fact.Reason!)})";
    }

    private static string Local(RouteInspectFact<RouteInspectAxiomsLocalState> fact)
    {
        return fact.State == RouteInspectFactState.Value
            ? RouteInspectHumanValues.LocalAxioms(fact.Value!)
            : $"{RouteInspectHumanValues.FactState(fact.State)} ({RouteInspectHumanValues.Text(fact.Reason!)})";
    }
}
