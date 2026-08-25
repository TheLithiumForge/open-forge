using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Route.List;

internal sealed class RouteListResult : ICliCommandResult
{
    private RouteListResult(
        CliSemanticStatus status,
        CliWorkspace? workspace,
        RouteListSelection selection,
        RouteListCoverage coverage,
        IReadOnlyList<RouteListRow> rows,
        IReadOnlyList<RouteListFinding> findings,
        CliNextAction? next)
    {
        Status = status;
        Workspace = workspace;
        Selection = selection;
        Coverage = coverage;
        Rows = rows;
        Findings = findings;
        Next = next;
    }

    internal int SchemaVersion => RouteListDefinitions.SchemaVersion;

    public string Command => RouteListDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    internal RouteListSelection Selection { get; }

    internal RouteListDepth? RequestedDepth => Coverage.RequestedDepth;

    internal RouteListDepth? EffectiveDepth => Coverage.EffectiveDepth;

    internal RouteListCoverage Coverage { get; }

    internal IReadOnlyList<RouteListRow> Rows { get; }

    internal IReadOnlyList<RouteListFinding> Findings { get; }

    public CliNextAction? Next { get; }

    internal static RouteListResult Create(
        CliSemanticStatus status,
        CliWorkspace? workspace,
        RouteListSelection selection,
        RouteListCoverage coverage,
        IEnumerable<RouteListRow> rows,
        IEnumerable<RouteListFinding> findings,
        CliNextAction? next)
    {
        _ = CliStatusDefinitions.Read(status);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(coverage);
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(findings);
        var materializedRows = rows.ToArray();
        if (materializedRows.Any(row => row is null))
        {
            throw new ArgumentException("Route-list rows cannot contain null.", nameof(rows));
        }

        var materializedFindings = findings.ToArray();
        if (materializedFindings.Any(finding => finding is null))
        {
            throw new ArgumentException("Route-list findings cannot contain null.", nameof(findings));
        }

        if (coverage.ConfirmedRowCount != materializedRows.Length)
        {
            throw new ArgumentException("Coverage confirmed-row count must match the concrete row count.", nameof(coverage));
        }

        ValidateWorkspace(status, workspace, materializedRows, materializedFindings);
        ValidateSelectionAndRows(status, selection, coverage, materializedRows);
        ValidateStatus(status, coverage, materializedRows, materializedFindings, next);
        return new RouteListResult(
            status,
            workspace,
            selection,
            coverage,
            new ReadOnlyCollection<RouteListRow>(materializedRows),
            new ReadOnlyCollection<RouteListFinding>(materializedFindings),
            next);
    }

    private static void ValidateWorkspace(
        CliSemanticStatus status,
        CliWorkspace? workspace,
        IReadOnlyList<RouteListRow> rows,
        IReadOnlyList<RouteListFinding> findings)
    {
        if (workspace is not null)
        {
            return;
        }

        if (rows.Count != 0)
        {
            throw new ArgumentException("A route-list result with rows requires workspace identity.", nameof(workspace));
        }

        if (status is not (CliSemanticStatus.Invalid or CliSemanticStatus.Blocked))
        {
            throw new ArgumentException("This route-list status requires a selected workspace.", nameof(workspace));
        }

        if (findings.Any(finding => finding.Code is not (
                RouteListFindingCode.InvalidWorkspace
                or RouteListFindingCode.WorkspaceUnavailable)))
        {
            throw new ArgumentException(
                "Workspace absence is valid only for a workspace-selection finding.",
                nameof(workspace));
        }
    }

    private static void ValidateSelectionAndRows(
        CliSemanticStatus status,
        RouteListSelection selection,
        RouteListCoverage coverage,
        IReadOnlyList<RouteListRow> rows)
    {
        if (status is CliSemanticStatus.Complete or CliSemanticStatus.Attention
            && !selection.IsResolved)
        {
            throw new ArgumentException("Complete route-list coverage requires a resolved selection.", nameof(selection));
        }

        if (!selection.IsResolved && rows.Count != 0)
        {
            throw new ArgumentException("An unresolved source selection cannot produce route rows.", nameof(selection));
        }

        var rootRows = rows.Where(row => row.RelativeDepth == 0).ToArray();
        if (rootRows.Length > coverage.SelectedRootCount)
        {
            throw new ArgumentException("Emitted root rows cannot exceed selected-root coverage.", nameof(rows));
        }

        if (status is CliSemanticStatus.Complete or CliSemanticStatus.Attention
            && rootRows.Length != coverage.SelectedRootCount)
        {
            throw new ArgumentException("Complete coverage requires every selected root row.", nameof(rows));
        }

        if (selection.Kind == RouteListSelectionKind.LoaderRoots)
        {
            if (rootRows.Any(row => row.Provenance.Selection != RouteListSelectionProvenance.LoaderRoot))
            {
                throw new ArgumentException("Loader-root selection requires Loader-root row provenance.", nameof(rows));
            }
        }
        else
        {
            if (coverage.SelectedRootCount > 1
                || rootRows.Any(row => row.Provenance.Selection is not (
                    RouteListSelectionProvenance.ExplicitRoot
                    or RouteListSelectionProvenance.DetachedRoot)))
            {
                throw new ArgumentException("Explicit source selection requires one explicit or detached result root.", nameof(rows));
            }

            if (rootRows.Any(row =>
                    !string.Equals(row.Id, selection.ResolvedId, StringComparison.Ordinal)
                    || !string.Equals(row.Path, selection.ResolvedPath, StringComparison.Ordinal)))
            {
                throw new ArgumentException("The explicit result root must match the resolved source identity.", nameof(rows));
            }

            if (status is CliSemanticStatus.Complete or CliSemanticStatus.Attention
                && (coverage.SelectedRootCount != 1 || rootRows.Length != 1))
            {
                throw new ArgumentException("Complete explicit selection requires exactly one matching root row.", nameof(rows));
            }
        }

        ValidateDepthAndOrdering(status, coverage, rows);
    }

    private static void ValidateDepthAndOrdering(
        CliSemanticStatus status,
        RouteListCoverage coverage,
        IReadOnlyList<RouteListRow> rows)
    {
        if (coverage.RequestedDepth is { Kind: RouteListDepthKind.Finite, Value: { } requested }
            && rows.Any(row => row.RelativeDepth > requested))
        {
            throw new ArgumentException("A route row exceeds the requested structural depth.", nameof(rows));
        }

        if (coverage.RequestedDepth is { Kind: RouteListDepthKind.Finite, Value: { } finiteRequested }
            && coverage.EffectiveDepth is { } effective
            && (effective.Kind == RouteListDepthKind.All || effective.Value > finiteRequested))
        {
            throw new ArgumentException("Effective depth cannot exceed a finite requested depth.", nameof(coverage));
        }

        var indexByPath = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var index = 0; index < rows.Count; index++)
        {
            if (!indexByPath.TryAdd(rows[index].Path, index))
            {
                throw new ArgumentException("Route-list row paths must be unique.", nameof(rows));
            }
        }

        var lastPathByParent = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var siblingKey = row.ParentPath ?? string.Empty;
            if (lastPathByParent.TryGetValue(siblingKey, out var priorSiblingPath)
                && string.CompareOrdinal(priorSiblingPath, row.Path) >= 0)
            {
                throw new ArgumentException("Route-list roots and siblings must use canonical path order.", nameof(rows));
            }

            lastPathByParent[siblingKey] = row.Path;

            if (row.ParentPath is not null
                && indexByPath.TryGetValue(row.ParentPath, out var establishedParentIndex)
                && establishedParentIndex >= index)
            {
                throw new ArgumentException("An emitted routed parent must occur before its child.", nameof(rows));
            }

            if (row.Provenance.Selection != RouteListSelectionProvenance.Descendant)
            {
                continue;
            }

            if (row.ParentPath is not { } parentPath
                || !indexByPath.TryGetValue(parentPath, out var parentIndex)
                || parentIndex >= index)
            {
                throw new ArgumentException("A descendant row requires its parent earlier in the result.", nameof(rows));
            }

            var parent = rows[parentIndex];
            if (!string.Equals(row.ParentId, parent.Id, StringComparison.Ordinal)
                || row.RelativeDepth != parent.RelativeDepth + 1
                || (row.AbsoluteDepth is null) != (parent.AbsoluteDepth is null)
                || row.AbsoluteDepth is { } childAbsolute
                && parent.AbsoluteDepth is { } parentAbsolute
                && childAbsolute != parentAbsolute + 1)
            {
                throw new ArgumentException("A descendant row depth does not follow its parent.", nameof(rows));
            }
        }

        if (status is not (CliSemanticStatus.Complete or CliSemanticStatus.Attention))
        {
            return;
        }

        var effectiveDepth = coverage.EffectiveDepth
            ?? throw new ArgumentException("Complete coverage requires effective depth.", nameof(coverage));
        if (effectiveDepth.Kind == RouteListDepthKind.Finite
            && rows.Any(row => row.RelativeDepth > effectiveDepth.FiniteValue))
        {
            throw new ArgumentException("A route row exceeds the effective structural depth.", nameof(rows));
        }

        var requestedDepth = coverage.RequestedDepth
            ?? throw new ArgumentException("Complete coverage requires requested depth.", nameof(coverage));
        foreach (var entrypoint in rows.Where(row => row.Kind == RouteListRowKind.Entrypoint))
        {
            var childrenWereRequested = requestedDepth.Kind == RouteListDepthKind.All
                || entrypoint.RelativeDepth < requestedDepth.FiniteValue;
            if (!childrenWereRequested)
            {
                continue;
            }

            if (entrypoint.DirectChildCount is not { } directChildCount)
            {
                throw new ArgumentException("Complete child coverage requires an entrypoint child count.", nameof(rows));
            }

            var emittedChildren = rows.Count(row =>
                string.Equals(row.ParentPath, entrypoint.Path, StringComparison.Ordinal));
            var effectiveBoundaryIncludesChildren = effectiveDepth.Kind == RouteListDepthKind.All
                || entrypoint.RelativeDepth < effectiveDepth.FiniteValue;
            if ((effectiveBoundaryIncludesChildren && emittedChildren != directChildCount)
                || (!effectiveBoundaryIncludesChildren && directChildCount != 0))
            {
                throw new ArgumentException("Complete child coverage must emit every direct routed child.", nameof(rows));
            }
        }
    }

    private static void ValidateStatus(
        CliSemanticStatus status,
        RouteListCoverage coverage,
        IReadOnlyList<RouteListRow> rows,
        IReadOnlyList<RouteListFinding> findings,
        CliNextAction? next)
    {
        var statusRule = RouteListDefinitions.ReadFindingResultStatusRule(status);
        switch (status)
        {
            case CliSemanticStatus.Complete:
                RequireCoverage(coverage, RouteListCoverageState.Complete);
                RequireNext(next, required: false);
                break;
            case CliSemanticStatus.Attention:
                RequireCoverage(coverage, RouteListCoverageState.Complete);
                break;
            case CliSemanticStatus.Incomplete:
                RequireCoverage(coverage, RouteListCoverageState.Incomplete);
                RequireNext(next, required: true);
                break;
            case CliSemanticStatus.Invalid:
                RequireCoverage(coverage, RouteListCoverageState.NotStarted);
                if (rows.Count != 0)
                {
                    throw new ArgumentException("Invalid route-list input cannot produce rows.", nameof(rows));
                }

                RequireNext(next, required: true);
                break;
            case CliSemanticStatus.Blocked:
                RequireCoverage(coverage, RouteListCoverageState.Blocked);
                RequireNext(next, required: true);
                break;
            case CliSemanticStatus.Failed:
                RequireCoverage(coverage, RouteListCoverageState.Failed);
                RequireNext(next, required: true);
                break;
            case CliSemanticStatus.Interrupted:
                RequireCoverage(coverage, RouteListCoverageState.Interrupted);
                RequireNext(next, required: true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, "The route-list status is not defined.");
        }

        RequireFindingSet(findings, statusRule);
    }

    private static void RequireCoverage(RouteListCoverage coverage, RouteListCoverageState expected)
    {
        if (coverage.State != expected)
        {
            throw new ArgumentException($"The route-list status requires {expected} coverage.", nameof(coverage));
        }
    }

    private static void RequireFindingSet(
        IReadOnlyList<RouteListFinding> findings,
        RouteListFindingResultStatusRule statusRule)
    {
        if (statusRule.RequiredFindingStatus is null)
        {
            if (findings.Count != 0)
            {
                throw new ArgumentException("Complete route-list results cannot contain findings.", nameof(findings));
            }

            return;
        }

        if (!findings.Any(finding => finding.Status == statusRule.RequiredFindingStatus.Value))
        {
            throw new ArgumentException(
                $"The route-list result requires a {statusRule.RequiredFindingStatus.Value} finding.",
                nameof(findings));
        }

        if (findings.Any(finding => !statusRule.AllowedFindingStatuses.Contains(finding.Status)))
        {
            throw new ArgumentException("A route-list finding conflicts with the result status.", nameof(findings));
        }
    }

    private static void RequireNext(CliNextAction? next, bool required)
    {
        if (required && next is null)
        {
            throw new ArgumentException("The route-list result requires one next action.", nameof(next));
        }

        if (!required && next is not null)
        {
            throw new ArgumentException("The route-list result cannot contain a next action.", nameof(next));
        }
    }
}
