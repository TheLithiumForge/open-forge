using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static class RouteListExpandedRenderer
{
    internal static string Render(RouteListResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var lines = new List<string>
        {
            "Open Forge route list",
            $"Result: {Status(result.Status)}",
            $"Coverage: {CoverageState(result.Coverage.State)}",
            $"Workspace: {Optional(result.Workspace?.LexicalRoot)}",
            $"Selected by: {SelectedBy(result.Workspace)}",
            $"Selection: {Selection(result.Selection)}",
            $"Selected roots: {result.Coverage.SelectedRootCount}",
            $"Requested depth: {Depth(result.RequestedDepth)}",
            $"Effective depth: {Depth(result.EffectiveDepth)}",
            $"Depth explanation: {Escape(DepthExplanation(result.RequestedDepth))}",
        };
        AddCoverage(lines, result);
        AddFindings(lines, result);
        AddNext(lines, result);
        lines.Add($"Rows: {result.Rows.Count}");
        AddRows(lines, result);
        return string.Join(Environment.NewLine, lines);
    }

    private static void AddCoverage(ICollection<string> lines, RouteListResult result)
    {
        lines.Add("Coverage evidence:");
        if (result.Coverage.Evidence.Count == 0)
        {
            lines.Add("  none");
        }
        else
        {
            foreach (var evidence in result.Coverage.Evidence)
            {
                lines.Add($"  - {Escape(evidence)}");
            }
        }

        lines.Add("Unresolved boundaries:");
        if (result.Coverage.UnresolvedBoundaries.Count == 0)
        {
            lines.Add("  none");
        }
        else
        {
            foreach (var boundary in result.Coverage.UnresolvedBoundaries)
            {
                lines.Add($"  - {Escape(boundary)}");
            }
        }
    }

    private static void AddRows(ICollection<string> lines, RouteListResult result)
    {
        for (var index = 0; index < result.Rows.Count; index++)
        {
            var row = result.Rows[index];
            lines.Add($"Row {index + 1}:");
            var indentation = new string(' ', checked(row.RelativeDepth * 2));
            AddRowFact(lines, indentation, "ID", row.Id);
            AddRowFact(lines, indentation, "Path", row.Path);
            AddRowFact(lines, indentation, "Parent ID", Optional(row.ParentId));
            AddRowFact(lines, indentation, "Parent path", Optional(row.ParentPath));
            AddRowFact(lines, indentation, "Absolute depth", Number(row.AbsoluteDepth));
            AddRowFact(lines, indentation, "Relative depth", row.RelativeDepth.ToString(System.Globalization.CultureInfo.InvariantCulture));
            AddRowFact(lines, indentation, "Kind", RowKind(row.Kind));
            AddRowFact(lines, indentation, "Direct children", Number(row.DirectChildCount));
            AddRowFact(lines, indentation, "Description", Escape(row.Description));
            AddRowFact(lines, indentation, "Tags", Values(row.Tags));
            AddRowFact(lines, indentation, "Provenance", Provenance(row.Provenance));
        }
    }

    private static void AddRowFact(
        ICollection<string> lines,
        string indentation,
        string label,
        string value)
    {
        lines.Add($"{indentation}{label}: {value}");
    }

    private static void AddFindings(ICollection<string> lines, RouteListResult result)
    {
        lines.Add($"Findings: {result.Findings.Count}");
        for (var index = 0; index < result.Findings.Count; index++)
        {
            var finding = result.Findings[index];
            lines.Add($"Finding {index + 1}:");
            lines.Add($"  Code: {finding.MachineCode}");
            lines.Add($"  Status: {Status(finding.Status)}");
            lines.Add($"  Path: {Optional(finding.Subject)}");
            lines.Add($"  Message: {Escape(finding.Cause)}");
            lines.Add($"  Candidates: {Values(finding.CandidatePaths)}");
        }
    }

    private static void AddNext(ICollection<string> lines, RouteListResult result)
    {
        if (result.Next is null)
        {
            return;
        }

        lines.Add("Next:");
        lines.Add($"  Command: {Escape(result.Next.Command)}");
        lines.Add($"  Reason: {Escape(result.Next.Reason)}");
    }

    private static string Selection(RouteListSelection selection)
    {
        return selection.Kind switch
        {
            RouteListSelectionKind.LoaderRoots => "loader roots from the authored Loader",
            RouteListSelectionKind.SourceId =>
                $"source ID {Optional(selection.AttemptedId)} -> {Optional(selection.ResolvedId)} at {Optional(selection.ResolvedPath)}",
            RouteListSelectionKind.SourcePath =>
                $"source path {Optional(selection.AttemptedPath)} -> {Optional(selection.ResolvedId)} at {Optional(selection.ResolvedPath)}",
            _ => throw new ArgumentOutOfRangeException(nameof(selection), selection.Kind, "The route-list selection kind is not defined."),
        };
    }

    private static string Provenance(RouteListProvenance provenance)
    {
        var overwrite = provenance.HasOverwrite ? "; overwrite present" : string.Empty;
        return $"{SelectionProvenance(provenance.Selection)}; {SourceProvenance(provenance.Source)}{overwrite}";
    }

    private static string Values(IEnumerable<string> values)
    {
        return string.Concat(
            "[",
            string.Join(", ", values.Select(value => string.Concat("\"", Escape(value), "\""))),
            "]");
    }

    private static string Optional(string? value)
    {
        return value is null ? "none" : Escape(value);
    }

    private static string Number(int? value)
    {
        return value?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "not applicable";
    }

    private static string Depth(RouteListDepth? depth)
    {
        return depth?.MachineValue ?? "not established";
    }

    private static string Status(CliSemanticStatus status)
    {
        return CliStatusDefinitions.Read(status).MachineName;
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

    private static string RowKind(RouteListRowKind kind)
    {
        return kind switch
        {
            RouteListRowKind.Entrypoint => "entrypoint",
            RouteListRowKind.RoutedLeaf => "routed-leaf",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The route-list row kind is not defined."),
        };
    }

    private static string SelectionProvenance(RouteListSelectionProvenance provenance)
    {
        return provenance switch
        {
            RouteListSelectionProvenance.LoaderRoot => "Loader root",
            RouteListSelectionProvenance.ExplicitRoot => "explicit root",
            RouteListSelectionProvenance.DetachedRoot => "detached root",
            RouteListSelectionProvenance.Descendant => "descendant",
            _ => throw new ArgumentOutOfRangeException(nameof(provenance), provenance, "The route-list selection provenance is not defined."),
        };
    }

    private static string SourceProvenance(RouteListSourceProvenance provenance)
    {
        return provenance switch
        {
            RouteListSourceProvenance.AuthoredEntrypoint => "authored topology entrypoint",
            RouteListSourceProvenance.AuthoredLeaf => "authored topology leaf",
            RouteListSourceProvenance.RoutedNative => "routed native source",
            _ => throw new ArgumentOutOfRangeException(nameof(provenance), provenance, "The route-list source provenance is not defined."),
        };
    }

    private static string SelectedBy(CliWorkspace? workspace)
    {
        if (workspace is null)
        {
            return "none";
        }

        return workspace.SelectedBy switch
        {
            CliWorkspaceSelectionMethod.CurrentDirectory => "current directory",
            CliWorkspaceSelectionMethod.ExplicitWorkspace => "--workspace",
            _ => throw new ArgumentOutOfRangeException(nameof(workspace), workspace.SelectedBy, "The workspace selection method is not defined."),
        };
    }

    private static string DepthExplanation(RouteListDepth? depth)
    {
        if (depth is null)
        {
            return "requested depth was not established";
        }

        if (depth.Kind == RouteListDepthKind.All)
        {
            return "complete routed descendant closure";
        }

        var finiteDepth = depth.Value
            ?? throw new InvalidOperationException("A finite route-list depth requires a value.");
        return finiteDepth switch
        {
            0 => "selected roots only",
            1 => "selected roots plus direct routed children",
            _ => $"selected roots and routed descendants through relative depth {finiteDepth}",
        };
    }

    private static string Escape(string value)
    {
        return RouteListTextEscaping.Escape(value);
    }
}
