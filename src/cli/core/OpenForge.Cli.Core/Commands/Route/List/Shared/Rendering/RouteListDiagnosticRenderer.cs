using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static class RouteListDiagnosticRenderer
{
    private const int MaximumDiagnosticValueLength = RouteListTextEscaping.DiagnosticValueLimit;

    internal static string? Render(CliPresentationRequest<RouteListResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        var result = presentation.Result;
        var lines = new List<string>
        {
            "route-list diagnostics",
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
        };
        AddWorkspace(lines, result.Workspace);
        AddSelection(lines, result.Selection);
        lines.Add($"coverage.state={CoverageState(result.Coverage.State)}");
        lines.Add($"coverage.requested-depth={Depth(result.RequestedDepth)}");
        lines.Add($"coverage.effective-depth={Depth(result.EffectiveDepth)}");
        lines.Add($"coverage.evidence-count={result.Coverage.Evidence.Count}");
        lines.Add($"coverage.boundary-count={result.Coverage.UnresolvedBoundaries.Count}");
        lines.Add($"rows={result.Rows.Count}");
        lines.Add($"findings={result.Findings.Count}");
        return string.Join(Environment.NewLine, lines);
    }

    private static void AddWorkspace(
        ICollection<string> lines,
        CliWorkspace? workspace)
    {
        if (workspace is null)
        {
            lines.Add("workspace=none");
            return;
        }

        lines.Add($"workspace.lexical={Value(workspace.LexicalRoot)}");
        lines.Add($"workspace.physical={Value(workspace.PhysicalRoot)}");
    }

    private static void AddSelection(
        ICollection<string> lines,
        RouteListSelection selection)
    {
        lines.Add($"selection.kind={SelectionKind(selection.Kind)}");
        if (selection.AttemptedId is not null)
        {
            lines.Add($"selection.attempted-id={Value(selection.AttemptedId)}");
        }

        if (selection.AttemptedPath is not null)
        {
            lines.Add($"selection.attempted-path={Value(selection.AttemptedPath)}");
        }

        if (selection.ResolvedId is not null)
        {
            lines.Add($"selection.resolved-id={Value(selection.ResolvedId)}");
        }

        if (selection.ResolvedPath is not null)
        {
            lines.Add($"selection.resolved-path={Value(selection.ResolvedPath)}");
        }
    }

    private static string Value(string value)
    {
        return RouteListTextEscaping.Escape(value, MaximumDiagnosticValueLength);
    }

    private static string Depth(RouteListDepth? depth)
    {
        return depth?.MachineValue ?? "none";
    }

    private static string SelectionKind(RouteListSelectionKind kind)
    {
        return kind switch
        {
            RouteListSelectionKind.LoaderRoots => "loader-roots",
            RouteListSelectionKind.SourceId => "source-id",
            RouteListSelectionKind.SourcePath => "source-path",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The route-list selection kind is not defined."),
        };
    }

    private static string CoverageState(RouteListCoverageState state)
    {
        return state switch
        {
            RouteListCoverageState.NotStarted => "not-started",
            RouteListCoverageState.Complete => "complete",
            RouteListCoverageState.Incomplete => "incomplete",
            RouteListCoverageState.Blocked => "blocked",
            RouteListCoverageState.Failed => "failed",
            RouteListCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The route-list coverage state is not defined."),
        };
    }
}
