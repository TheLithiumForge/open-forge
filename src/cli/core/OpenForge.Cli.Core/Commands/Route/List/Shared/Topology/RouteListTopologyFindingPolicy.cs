using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed record RouteListTopologyFindingAtDepth(
    RouteListFinding Finding,
    int RelativeDepth);

internal static class RouteListTopologyFindingPolicy
{
    internal static RouteListFinding FromSelectionIssue(RouteListSelectionIssue issue)
    {
        return new RouteListFinding(
            issue.Code,
            issue.Status,
            issue.Subject,
            issue.Cause,
            issue.CandidatePaths);
    }

    internal static RouteListFinding FromFilesystem(RouteListFilesystemFinding finding)
    {
        return new RouteListFinding(
            finding.Code,
            finding.Status,
            finding.CanonicalLogicalSubject,
            finding.Cause);
    }

    internal static RouteListFinding FromRouteIssue(SourceRouteIssue issue)
    {
        return issue.Code switch
        {
            SourceRouteIssueCode.LoaderMalformed or SourceRouteIssueCode.LoaderDuplicateRoot => new RouteListFinding(
                RouteListFindingCode.LoaderMalformed,
                CliSemanticStatus.Incomplete,
                issue.CanonicalPath,
                issue.Cause),
            SourceRouteIssueCode.LoaderUnsafe => new RouteListFinding(
                RouteListFindingCode.PhysicalBoundary,
                CliSemanticStatus.Blocked,
                issue.CanonicalPath,
                issue.Cause),
            SourceRouteIssueCode.RouteAmbiguous => new RouteListFinding(
                RouteListFindingCode.RouteAmbiguous,
                CliSemanticStatus.Blocked,
                issue.CanonicalPath,
                issue.Cause,
                issue.RelatedPaths),
            SourceRouteIssueCode.LoaderUnavailable
                or SourceRouteIssueCode.LoaderUnreadable
                or SourceRouteIssueCode.LoaderDestinationMissing
                or SourceRouteIssueCode.RouteSupportUnavailable => new RouteListFinding(
                RouteListFindingCode.LoaderUnavailable,
                CliSemanticStatus.Incomplete,
                issue.CanonicalPath,
                issue.Cause),
            _ => throw new ArgumentOutOfRangeException(nameof(issue), issue.Code, "The source route issue code is not defined."),
        };
    }

    internal static IReadOnlyList<RouteListTopologyFindingAtDepth> ReadRelevantInventoryFindings(
        RouteListTopologyInput input,
        SourceRouteTopology topology,
        IReadOnlyList<SourceRouteNode> selectedRoots)
    {
        var relevant = new List<RouteListTopologyFindingAtDepth>();
        foreach (var finding in input.Inventory.Findings)
        {
            var depth = RouteListTopologyFindingDepthPolicy.ReadRelevantDepth(
                input,
                topology,
                selectedRoots,
                finding);
            if (depth is null && finding.Status != CliSemanticStatus.Interrupted)
            {
                continue;
            }

            relevant.Add(new RouteListTopologyFindingAtDepth(
                FromFilesystem(finding),
                depth ?? 0));
        }

        return relevant;
    }

    internal static bool AffectsDirectChildren(
        SourceRouteNode node,
        RouteSource source,
        RouteListFilesystemFinding finding)
    {
        if (finding.Status == CliSemanticStatus.Attention
            || source.Kind != RouteSourceKind.Entrypoint)
        {
            return false;
        }

        return RouteListTopologyFindingDepthPolicy.ReadLexicalRelativeDepth(
            node,
            finding.CanonicalLogicalSubject) == 1;
    }

    internal static IReadOnlyList<RouteListFinding> OrderDistinct(
        IEnumerable<RouteListFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);
        return findings
            .Select(finding => finding ?? throw new ArgumentException("Topology findings cannot contain null.", nameof(findings)))
            .GroupBy(
                finding => string.Join(
                    '\0',
                    finding.MachineCode,
                    finding.Status,
                    finding.Subject,
                    finding.Cause,
                    string.Join('\0', finding.CandidatePaths)),
                StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(finding => finding.Subject, StringComparer.Ordinal)
            .ThenBy(finding => finding.MachineCode, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .ToArray();
    }

    internal static CliSemanticStatus ReadAggregateStatus(
        RouteListSelectionResolutionState selectionState,
        IReadOnlyList<RouteListFinding> findings)
    {
        if (selectionState == RouteListSelectionResolutionState.Invalid)
        {
            return CliSemanticStatus.Invalid;
        }

        if (findings.Any(finding => finding.Status == CliSemanticStatus.Invalid))
        {
            return CliSemanticStatus.Invalid;
        }

        if (selectionState == RouteListSelectionResolutionState.Interrupted
            || findings.Any(finding => finding.Status == CliSemanticStatus.Interrupted))
        {
            return CliSemanticStatus.Interrupted;
        }

        if (findings.Any(finding => finding.Status == CliSemanticStatus.Failed))
        {
            return CliSemanticStatus.Failed;
        }

        if (selectionState == RouteListSelectionResolutionState.Blocked
            || findings.Any(finding => finding.Status == CliSemanticStatus.Blocked))
        {
            return CliSemanticStatus.Blocked;
        }

        if (selectionState == RouteListSelectionResolutionState.Incomplete
            || findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete))
        {
            return CliSemanticStatus.Incomplete;
        }

        return findings.Any(finding => finding.Status == CliSemanticStatus.Attention)
            ? CliSemanticStatus.Attention
            : CliSemanticStatus.Complete;
    }

    internal static string ReadBoundary(RouteListFinding finding)
    {
        return finding.Subject is null
            ? finding.Cause
            : $"{finding.Subject}: {finding.Cause}";
    }
}
