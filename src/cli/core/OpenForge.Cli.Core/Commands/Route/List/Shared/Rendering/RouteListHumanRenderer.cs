using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using static OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering.RouteListHumanValues;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static class RouteListHumanRenderer
{
    internal static string Render(CliPresentationRequest<RouteListResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var style = CliHumanStyle.For(presentation);
        var expanded = presentation.Presentation.View == CliView.Expanded;
        var heading = result.Selection.ResolvedId is { } id ? $"{style.Information("Routes under")} {Escape(id)}" : style.Information("Routes");
        var lines = new List<string>
        {
            heading,
            $"{style.Information("Status:")} {style.Status(CliHumanText.Status(result.Status), result.Status)}",
            $"Workspace: {Optional(result.Workspace?.LexicalRoot)}",
            $"Selected by: {SelectedBy(result.Workspace)}",
            $"Coverage: {CoverageState(result.Coverage.State)}; roots: {result.Coverage.SelectedRootCount}; depth: {Depth(result.EffectiveDepth)}; routes: {result.Rows.Count}",
        };
        if (expanded)
        {
            lines.Add($"Selection: {RouteListHumanValues.Selection(result.Selection)}");
            lines.Add($"Requested depth: {Depth(result.RequestedDepth)} ({DepthExplanation(result.RequestedDepth)})");
            foreach (var evidence in result.Coverage.Evidence)
            {
                lines.Add($"Confirmed: {Escape(evidence)}");
            }
        }
        foreach (var boundary in result.Coverage.UnresolvedBoundaries)
        {
            lines.Add($"Unresolved: {Escape(boundary)}");
        }
        foreach (var finding in result.Findings)
        {
            lines.Add($"{style.Finding(finding.Status)}: {Escape(finding.Cause)} [{finding.MachineCode}]");
            if (finding.Subject is { } subject)
            {
                lines.Add($"  Subject: {Escape(subject)}");
            }
            foreach (var candidate in finding.CandidatePaths)
            {
                lines.Add($"  Candidate: {Escape(candidate)}");
            }
        }
        if (result.Next is { } next)
        {
            lines.Add($"{style.Information("Next:")} {CliHumanText.Text(next.Command)}");
            if (expanded)
            {
                lines.Add(Escape(next.Reason));
            }
        }
        lines.Add(string.Empty);
        if (result.Rows.Count == 0)
        {
            lines.Add(result.Coverage.State == RouteListCoverageState.Complete
                ? "No routes found."
                : "No routes established; the listing is not complete.");
        }
        foreach (var row in result.Rows)
        {
            RouteListRowsHumanRenderer.Add(lines, row, expanded);
        }
        return string.Join(Environment.NewLine, lines);
    }
}
