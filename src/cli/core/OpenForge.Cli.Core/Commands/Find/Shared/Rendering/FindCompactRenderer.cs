using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindCompactRenderer
{
    internal static string Render(FindResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        CliOperationStage.ValidateResult(result);

        var lines = new List<string>
        {
            Summary(result),
        };
        foreach (var match in result.Matches)
        {
            lines.Add($"{FindTextEscaping.Escape(match.Id)}\t{FindTextEscaping.Escape(match.Path)}");
        }

        if (result.Presentation.Content.IsRequested)
        {
            FindExpandedRenderer.AddProjectionBlocks(lines, result.Matches);
        }

        if (result.Status == CliSemanticStatus.Complete && result.Matches.Count == 0)
        {
            lines.Add("No matches.");
        }

        AddFindings(lines, result);
        if (FindExpandedRenderer.ReadNextLine(result) is { } next)
        {
            lines.Add(next);
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string Summary(FindResult result)
    {
        var projection = result.Presentation.Content.IsRequested
            ? $"\tprojection={ProjectionCoverage(result.Coverage.Projection)}"
            : string.Empty;
        return $"result={Status(result.Status)}\tcoverage={Coverage(result.Coverage.State)}"
            + $"{projection}\tuniverse={UniverseMode(result.Universe.Mode)}\tmatches={result.Matches.Count}";
    }

    private static void AddFindings(ICollection<string> lines, FindResult result)
    {
        foreach (var finding in result.Findings)
        {
            var subject = finding.Subject is null
                ? "none"
                : FindTextEscaping.Escape(finding.Subject);
            lines.Add(
                $"finding code={FindDefinitions.ReadFindingCode(finding.Code)} "
                + $"status={Status(finding.Status)} subject={subject} "
                + $"cause=\"{FindTextEscaping.Escape(finding.Cause)}\" "
                + $"candidates={Candidates(finding.Candidates)}");
        }
    }

    private static string Candidates(IEnumerable<FindSourceIdentity> candidates)
    {
        var values = candidates
            .Select(candidate =>
                $"{FindTextEscaping.Escape(candidate.Id)} -> {FindTextEscaping.Escape(candidate.Path)}")
            .ToArray();
        return values.Length == 0
            ? "none"
            : $"[{string.Join(", ", values)}]";
    }

    private static string Status(CliSemanticStatus status)
        => CliStatusDefinitions.Read(status).MachineName;

    private static string Coverage(FindCoverageState state)
        => state switch
        {
            FindCoverageState.NotStarted => "not-started",
            FindCoverageState.Complete => "complete",
            FindCoverageState.Incomplete => "incomplete",
            FindCoverageState.Blocked => "blocked",
            FindCoverageState.Failed => "failed",
            FindCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Find coverage state is not defined."),
        };

    private static string ProjectionCoverage(FindProjectionCoverageState state)
        => state switch
        {
            FindProjectionCoverageState.NotRequested => "not-requested",
            FindProjectionCoverageState.NotStarted => "not-started",
            FindProjectionCoverageState.Complete => "complete",
            FindProjectionCoverageState.Incomplete => "incomplete",
            FindProjectionCoverageState.Blocked => "blocked",
            FindProjectionCoverageState.Failed => "failed",
            FindProjectionCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Find projection coverage state is not defined."),
        };

    private static string UniverseMode(FindUniverseMode mode)
        => mode switch
        {
            FindUniverseMode.Default => "default",
            FindUniverseMode.Filtered => "filtered",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "The Find universe mode is not defined."),
        };
}
