using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static class RouteListCompactRenderer
{
    internal static string Render(RouteListResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var lines = new List<string>
        {
            $"result={Status(result.Status)}  coverage={CoverageState(result.Coverage.State)}  roots={result.Coverage.SelectedRootCount}  depth={Depth(result.EffectiveDepth)}  rows={result.Rows.Count}",
            $"workspace={Optional(result.Workspace?.LexicalRoot)} selectedBy={SelectedBy(result.Workspace)} selection={Selection(result.Selection)}",
            $"requestedDepth={Depth(result.RequestedDepth)} effectiveDepth={Depth(result.EffectiveDepth)}",
            $"depth-explanation=\"{Escape(DepthExplanation(result.RequestedDepth))}\"",
        };
        AddCoverage(lines, result);
        AddFindings(lines, result);
        AddNext(lines, result);
        AddRows(lines, result);
        return string.Join(Environment.NewLine, lines);
    }

    private static void AddCoverage(ICollection<string> lines, RouteListResult result)
    {
        if (result.Coverage.Evidence.Count == 0)
        {
            lines.Add("coverage-evidence=none");
        }
        else
        {
            lines.Add($"coverage-evidence=\"{Escape(string.Join("; ", result.Coverage.Evidence))}\"");
        }

        if (result.Coverage.UnresolvedBoundaries.Count == 0)
        {
            lines.Add("coverage-boundaries=none");
        }
        else
        {
            lines.Add($"coverage-boundaries=\"{Escape(string.Join("; ", result.Coverage.UnresolvedBoundaries))}\"");
        }
    }

    private static void AddRows(ICollection<string> lines, RouteListResult result)
    {
        foreach (var row in result.Rows)
        {
            var indentation = new string(' ', checked(row.RelativeDepth * 2));
            lines.Add(
                $"{indentation}{Escape(row.Id)}  {Escape(row.Path)}  "
                + $"description=\"{Escape(row.Description)}\" tags={Values(row.Tags)} "
                + $"parentId={Optional(row.ParentId)} parentPath={Optional(row.ParentPath)} "
                + $"absoluteDepth={Number(row.AbsoluteDepth)} relativeDepth={row.RelativeDepth} "
                + $"kind={RowKind(row.Kind)} directChildCount={Number(row.DirectChildCount)} "
                + $"provenance={Provenance(row.Provenance)}");
        }
    }

    private static void AddFindings(ICollection<string> lines, RouteListResult result)
    {
        lines.Add($"findings={result.Findings.Count}");
        foreach (var finding in result.Findings)
        {
            lines.Add(
                $"finding code={finding.MachineCode} status={Status(finding.Status)} "
                + $"path={Optional(finding.Subject)} message=\"{Escape(finding.Cause)}\" "
                + $"candidates={Values(finding.CandidatePaths)}");
        }
    }

    private static void AddNext(ICollection<string> lines, RouteListResult result)
    {
        if (result.Next is null)
        {
            return;
        }

        lines.Add($"next command={Escape(result.Next.Command)} reason=\"{Escape(result.Next.Reason)}\"");
    }

    private static string Selection(RouteListSelection selection)
    {
        var identity = selection.Kind switch
        {
            RouteListSelectionKind.LoaderRoots => "loader-roots",
            RouteListSelectionKind.SourceId => $"source-id attemptedId={Optional(selection.AttemptedId)} resolvedId={Optional(selection.ResolvedId)} resolvedPath={Optional(selection.ResolvedPath)}",
            RouteListSelectionKind.SourcePath => $"source-path attemptedPath={Optional(selection.AttemptedPath)} resolvedId={Optional(selection.ResolvedId)} resolvedPath={Optional(selection.ResolvedPath)}",
            _ => throw new ArgumentOutOfRangeException(nameof(selection), selection.Kind, "The route-list selection kind is not defined."),
        };
        return identity;
    }

    private static string Provenance(RouteListProvenance provenance)
    {
        var overwrite = provenance.HasOverwrite ? "+overwrite" : string.Empty;
        return $"{SelectionProvenance(provenance.Selection)}/{SourceProvenance(provenance.Source)}{overwrite}";
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
        return value is null ? "none" : $"\"{Escape(value)}\"";
    }

    private static string Number(int? value)
    {
        return value?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "none";
    }

    private static string Depth(RouteListDepth? depth)
    {
        return depth?.MachineValue ?? "none";
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
            RouteListSelectionProvenance.LoaderRoot => "loader-root",
            RouteListSelectionProvenance.ExplicitRoot => "explicit-root",
            RouteListSelectionProvenance.DetachedRoot => "detached-root",
            RouteListSelectionProvenance.Descendant => "descendant",
            _ => throw new ArgumentOutOfRangeException(nameof(provenance), provenance, "The route-list selection provenance is not defined."),
        };
    }

    private static string SourceProvenance(RouteListSourceProvenance provenance)
    {
        return provenance switch
        {
            RouteListSourceProvenance.AuthoredEntrypoint => "authored-entrypoint",
            RouteListSourceProvenance.AuthoredLeaf => "authored-leaf",
            RouteListSourceProvenance.RoutedNative => "routed-native",
            _ => throw new ArgumentOutOfRangeException(nameof(provenance), provenance, "The route-list source provenance is not defined."),
        };
    }

    private static string SelectedBy(CliWorkspace? workspace)
    {
        if (workspace is null)
        {
            return "none";
        }

        return WorkspaceSelectionWireVocabulary.Read(workspace.SelectedBy);
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
