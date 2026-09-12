using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindDiagnosticRenderer
{
    internal static string? Render(CliPresentationRequest<FindResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var lines = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"workspace={Workspace(result.Workspace)}",
            $"candidates={Count(result.Universe.CandidateCount)}",
            $"inspected={Count(result.Universe.InspectedCount)}",
            $"matches={result.Matches.Count}",
            $"coverage={Coverage(result.Coverage.State)}",
            $"matching={Coverage(result.Coverage.Matching)}",
            $"projection={ProjectionCoverage(result.Coverage.Projection)}",
            $"predicates={result.Query.EffectivePredicates.Count}",
            $"findings={result.Findings.Count}",
            $"next={(result.Next is null ? "none" : "present")}",
        };
        return string.Join(Environment.NewLine, lines);
    }

    private static string Workspace(CliWorkspace? workspace)
        => workspace is null
            ? "none"
            : FindTextEscaping.Escape(workspace.LexicalRoot, FindTextEscaping.DiagnosticValueLimit);

    private static string Count(int? count)
        => count?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "none";

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
}
