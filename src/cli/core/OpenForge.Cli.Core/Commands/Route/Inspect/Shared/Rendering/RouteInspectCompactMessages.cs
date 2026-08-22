using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectCompactMessages
{
    internal static void Add(
        ICollection<string> lines,
        RouteInspectResult result)
    {
        foreach (var observation in result.Observations)
        {
            lines.Add($"Observation: {RouteInspectHumanValues.Text(observation.Message)}");
        }

        if (result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention)
        {
            return;
        }

        foreach (var condition in result.Conditions)
        {
            lines.Add($"Condition: {RouteInspectHumanValues.Text(condition.Message)}");
            if (condition.Paths.Count != 0)
            {
                lines.Add($"Condition paths: {string.Join(", ", condition.Paths.Select(RouteInspectHumanValues.Text))}");
            }
        }
    }

    internal static void AddNext(
        ICollection<string> lines,
        RouteInspectResult result)
    {
        var next = RouteInspectHumanNext.Line(result);
        if (next is not null)
        {
            lines.Add(next);
        }
    }
}
