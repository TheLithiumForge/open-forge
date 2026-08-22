using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectCompactProfile
{
    internal static void Add(
        ICollection<string> lines,
        RouteInspectProfile? profile)
    {
        if (profile is null)
        {
            lines.Add("Reading behavior: not established");
            lines.Add("Own source: not established");
            lines.Add("Selecting this route adds: not established");
            lines.Add("Automatically read below it through #LoadNow: not established");
            lines.Add("Completeness: not established");
            lines.Add("Safety: not established");
            return;
        }

        AddReading(lines, profile.Reading);
        AddMeasurements(lines, profile.Measurements);
        AddTopology(lines, profile.Topology);
        lines.Add($"Completeness: {RouteInspectHumanValues.Completeness(profile.Completeness)}");
        lines.Add($"Safety: {RouteInspectHumanValues.Safety(profile.Safety)}");
    }

    private static void AddReading(
        ICollection<string> lines,
        RouteInspectReadingProfile reading)
    {
        lines.Add("Reading behavior");
        lines.Add($"Read at task start or resume: {BooleanFact(reading.TaskStart)}");
        if (reading.Automatic.State == RouteInspectFactState.Value)
        {
            foreach (var reason in reading.Automatic.Value!.Reasons)
            {
                if (reason.Kind == RouteInspectAutomaticReadingKind.OnDemand)
                {
                    lines.Add("Read automatically after another route: no");
                    lines.Add("Read when this route is selected: yes");
                }
                else
                {
                    lines.Add($"Read automatically when: {RouteInspectHumanAutomaticReading.Explanation(reason)}");
                }
            }
        }
        else
        {
            lines.Add($"Read automatically: {FactReason(reading.Automatic)}");
        }

        if (reading.Later.State == RouteInspectFactState.Value)
        {
            var later = reading.Later.Value!;
            lines.Add($"May be read again: {(later.MayBeReadAgain ? "yes" : "no")}");
            if (later.MayBeReadAgain)
            {
                lines.Add($"When: {RouteInspectHumanLaterReading.Description(later.Occasions)}");
            }
        }
        else
        {
            lines.Add($"May be read again: {FactReason(reading.Later)}");
        }
    }

    private static void AddMeasurements(
        ICollection<string> lines,
        RouteInspectMeasurements measurements)
    {
        lines.Add($"Own source: {RouteInspectHumanMeasurements.Measurement(measurements.OwnSource)}");
        lines.Add($"Selecting this route adds: {RouteInspectHumanMeasurements.Measurement(measurements.SelectionAddition)}");
        lines.Add(
            "Automatically read below it through #LoadNow: "
            + RouteInspectHumanMeasurements.Measurement(measurements.LoadNowDescendants));
    }

    private static void AddTopology(
        ICollection<string> lines,
        RouteInspectFact<RouteInspectTopology> fact)
    {
        if (fact.State != RouteInspectFactState.Value)
        {
            var display = $"{RouteInspectHumanValues.FactState(fact.State)} ({RouteInspectHumanValues.Text(fact.Reason!)})";
            lines.Add($"Route chain: {display}");
            lines.Add($"Parent: {display}");
            lines.Add($"Depth: {display}");
            lines.Add($"Direct children: {display}");
            lines.Add($"Descendants: {display}");
            return;
        }

        var topology = fact.Value!;
        var parent = topology.ParentId is null
            ? "none"
            : RouteInspectHumanValues.Text(topology.ParentId);
        lines.Add($"Route chain: {string.Join(" → ", topology.RouteChain.Select(RouteInspectHumanValues.Text))}");
        lines.Add($"Parent: {parent}");
        lines.Add($"Depth: {topology.Depth}");
        AddCounts(lines, topology.Counts);
    }

    private static void AddCounts(
        ICollection<string> lines,
        RouteInspectFact<RouteInspectTopologyCounts> counts)
    {
        if (counts.State != RouteInspectFactState.Value)
        {
            var display = RouteInspectHumanMeasurements.Fact(counts);
            lines.Add($"Direct children: {display}");
            lines.Add($"Descendants: {display}");
            return;
        }

        var value = counts.Value!;
        lines.Add($"Direct children: {value.DirectRoutedFileCount} files, {value.DirectEntrypointCount} entrypoints");
        lines.Add($"Descendants: {value.DescendantRoutedFileCount} files, {value.DescendantEntrypointCount} entrypoints");
    }

    private static string BooleanFact(RouteInspectFact<bool> fact)
    {
        if (fact.State == RouteInspectFactState.Value)
        {
            return fact.Value == true ? "yes" : "no";
        }

        return FactReason(fact);
    }

    private static string FactReason<T>(RouteInspectFact<T> fact)
    {
        return $"{RouteInspectHumanValues.FactState(fact.State)} ({RouteInspectHumanValues.Text(fact.Reason!)})";
    }
}
