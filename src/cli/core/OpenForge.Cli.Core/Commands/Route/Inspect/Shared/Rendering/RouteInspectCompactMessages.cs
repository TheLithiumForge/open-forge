using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectCompactMessages
{
    internal static void Add(ICollection<string> lines, RouteInspectResult result, CliHumanStyle style)
    {
        foreach (var observation in result.Observations)
        {
            lines.Add($"{style.Information("Note:")} {RouteInspectHumanValues.Text(observation.Message)} [{observation.MachineCode}]");
            AddSubject(lines, observation.Subject, observation.Paths);
        }
        foreach (var condition in result.Conditions)
        {
            lines.Add($"{style.Finding(condition.Status)}: {RouteInspectHumanValues.Text(condition.Message)} [{condition.MachineCode}]");
            AddSubject(lines, condition.Subject, condition.Paths);
        }
    }

    private static void AddSubject(ICollection<string> lines, string subject, IReadOnlyList<string> paths)
    {
        if (!paths.Contains(subject, StringComparer.Ordinal))
        {
            lines.Add($"  Subject: {RouteInspectHumanValues.Text(subject)}");
        }
        foreach (var path in paths)
        {
            lines.Add($"  Path: {RouteInspectHumanValues.Text(path)}");
        }
    }

    internal static void AddNext(ICollection<string> lines, RouteInspectResult result)
    {
        if (RouteInspectHumanNext.Line(result) is { } next)
        {
            lines.Add(next);
        }
    }
}
