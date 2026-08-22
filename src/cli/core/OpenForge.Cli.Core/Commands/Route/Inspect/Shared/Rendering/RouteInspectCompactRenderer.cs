using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectCompactRenderer
{
    internal static string Render(RouteInspectResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var lines = new List<string>();
        Add(lines, result, includeMessages: true);
        return string.Join(Environment.NewLine, lines);
    }

    internal static void Add(
        ICollection<string> lines,
        RouteInspectResult result,
        bool includeMessages)
    {
        lines.Add("Open Forge route inspect");
        lines.Add($"Workspace: {Workspace(result)}");
        lines.Add($"Selected by: {RouteInspectHumanValues.SelectedBy(result.Workspace)}");
        lines.Add($"Selection: {Selection(result.Selection)}");
        AddCandidates(lines, result.Selection);
        RouteInspectCompactIdentity.Add(lines, result.Identity);
        RouteInspectCompactProfile.Add(lines, result.Profile);
        if (includeMessages)
        {
            RouteInspectCompactMessages.Add(lines, result);
        }

        lines.Add($"Status: {RouteInspectHumanValues.Status(result.Status)}");
        RouteInspectCompactMessages.AddNext(lines, result);
    }

    private static string Workspace(RouteInspectResult result)
    {
        return result.Workspace is null
            ? "none"
            : RouteInspectHumanValues.Text(result.Workspace.LexicalRoot);
    }

    private static string Selection(RouteInspectSelection selection)
    {
        var requested = selection.RequestedReference is null
            ? "none"
            : $"\"{RouteInspectHumanValues.Text(selection.RequestedReference)}\"";
        return $"{RouteInspectHumanValues.ReferenceKind(selection.ReferenceKind)}; "
            + $"{RouteInspectHumanValues.SelectionMethod(selection.SelectionMethod)}; requested {requested}";
    }

    private static void AddCandidates(
        ICollection<string> lines,
        RouteInspectSelection selection)
    {
        if (selection.CandidatePaths.Count != 0)
        {
            lines.Add($"Candidates: {string.Join(", ", selection.CandidatePaths.Select(RouteInspectHumanValues.Text))}");
        }
    }
}
