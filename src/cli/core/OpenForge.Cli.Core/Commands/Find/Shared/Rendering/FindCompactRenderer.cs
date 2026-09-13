using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using static OpenForge.Cli.Core.Commands.Find.Shared.Rendering.FindHumanValues;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindCompactRenderer
{
    internal static string Render(FindResult result, CliHumanStyle style)
    {
        ArgumentNullException.ThrowIfNull(result);
        CliOperationStage.ValidateResult(result);

        var lines = new List<string>
        {
            Summary(result, style),
        };
        foreach (var match in result.Matches)
        {
            lines.Add($"{FindTextEscaping.Escape(match.Id)}\t{FindTextEscaping.Escape(match.Path)}");
        }

        if (result.Presentation.Content.IsRequested)
        {
            FindContentHumanRenderer.AddProjectionBlocks(lines, result.Matches);
        }

        if (result.Status == CliSemanticStatus.Complete && result.Matches.Count == 0)
        {
            lines.Add("No matches.");
        }

        FindFindingHumanRenderer.Add(lines, result, style);
        if (FindNextHumanRenderer.Line(result) is { } next)
        {
            lines.Add(next);
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string Summary(FindResult result, CliHumanStyle style)
    {
        var projection = result.Presentation.Content.IsRequested
            ? $"\tprojection={ProjectionCoverage(result.Coverage.Projection)}"
            : string.Empty;
        return $"result={style.Status(FindHumanValues.Status(result.Status), result.Status)}\tcoverage={Coverage(result.Coverage.State)}"
            + $"{projection}\tuniverse={UniverseMode(result.Universe.Mode)}\tmatches={result.Matches.Count}";
    }

}
