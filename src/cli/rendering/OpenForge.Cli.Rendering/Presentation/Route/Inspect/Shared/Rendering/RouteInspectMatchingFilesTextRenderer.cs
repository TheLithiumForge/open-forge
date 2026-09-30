using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.OutputText.Route.Inspect;

namespace OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Rendering;

internal static class RouteInspectMatchingFilesTextRenderer
{
    internal static void Render(StringBuilder builder, RouteInspectMatchingFilesData files)
    {
        if (files.Scope == RouteInspectMatchingFilesData.AllFilesScope)
        {
            builder.Append($"\n{RouteInspectText.MatchingAllFiles()}\n{files.Note}\n");
            return;
        }

        var count = files.Count?.ToString(CultureInfo.InvariantCulture) ?? RouteInspectText.MatchingUnavailable();
        var completeness = files.Complete ? RouteInspectText.MatchingComplete() : RouteInspectText.MatchingIncomplete();
        var scope = files.ScopeDescription ?? RouteInspectText.MatchingUnavailable();
        builder.Append($"\n{RouteInspectWording.MatchingFilesSummary(count, scope, completeness)}\n");
        if (files.Truncated)
        {
            builder.Append($"{RouteInspectWording.MatchingListedPaths(files.Paths.Count, files.Count)}\n");
        }

        foreach (var path in files.Paths)
        {
            builder.Append($"  {CliText.Escape(path)}\n");
        }

        if (files.Note is { } note)
        {
            builder.Append($"{note}\n");
        }

        if (files.Limitation is { } limitation)
        {
            builder.Append($"{limitation}\n");
        }
    }
}
