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
        RouteInspectCompactRenderer.Add(lines, result);
        lines.Add(string.Empty);
        AddExplanations(lines, result);
        AddAxioms(lines, result.Profile);
        AddAdditionalMeasurements(lines, result);
        RouteInspectCompactMessages.AddNext(lines, result);
        if (result.Next is { } next)
        {
            lines.Add(RouteInspectHumanValues.Text(next.Reason));
        }
        return string.Join(Environment.NewLine, lines);
    }

    private static void AddExplanations(
        ICollection<string> lines,
        RouteInspectResult result)
    {
        lines.Add("Explanations:");
        lines.Add($"  Why: {StatusReason(result.Status)}");
    }

    private static void AddAxioms(
        ICollection<string> lines,
        RouteInspectProfile? profile)
    {
        lines.Add("Applicable rules (Axioms):");
        if (profile is null)
        {
            lines.Add("  not established");
            return;
        }

        var axioms = profile.Axioms;
        if (axioms.State != RouteInspectFactState.Value)
        {
            lines.Add($"  {RouteInspectHumanValues.FactState(axioms.State)}: {RouteInspectHumanValues.Text(axioms.ReadReason())}");
            return;
        }

        var value = axioms.ReadValue();
        lines.Add($"  Inherited from: {Inherited(value.Inherited)}");
        lines.Add($"  Local: {Local(value.Local)}");
    }

    private static void AddAdditionalMeasurements(
        ICollection<string> lines,
        RouteInspectResult result)
    {
        if (result.Profile is { } profile)
        {
            lines.Add("Additional context size details:");
            lines.Add($"  Selected context: {RouteInspectHumanMeasurements.Measurement(profile.Measurements.SelectedClosure)}");
            lines.Add($"  Already in startup context: {RouteInspectHumanMeasurements.Measurement(profile.Measurements.TaskStartOverlap)}");
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
            var value = fact.ReadValue();
            return value.SourceIds.Count == 0
                ? "none"
                : string.Join(", ", value.SourceIds.Select(RouteInspectHumanValues.Text));
        }

        return $"{RouteInspectHumanValues.FactState(fact.State)} ({RouteInspectHumanValues.Text(fact.ReadReason())})";
    }

    private static string Local(RouteInspectFact<RouteInspectAxiomsLocalState> fact)
    {
        return fact.State == RouteInspectFactState.Value
            ? RouteInspectHumanValues.LocalAxioms(fact.ReadValue())
            : $"{RouteInspectHumanValues.FactState(fact.State)} ({RouteInspectHumanValues.Text(fact.ReadReason())})";
    }
}
