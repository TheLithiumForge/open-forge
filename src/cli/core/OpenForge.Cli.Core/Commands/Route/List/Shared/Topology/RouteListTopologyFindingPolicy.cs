using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed record RouteListTopologyFindingAtDepth(
    RouteListFinding Finding,
    int RelativeDepth);

internal static class RouteListTopologyFindingPolicy
{
    internal static RouteListFinding FromSelectionIssue(RouteListSelectionIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);
        return new RouteListFinding(
            issue.Code,
            issue.Status,
            issue.Subject,
            issue.Cause,
            issue.CandidatePaths);
    }

    internal static RouteListFinding FromFilesystem(RouteListFilesystemFinding finding)
    {
        ArgumentNullException.ThrowIfNull(finding);
        return new RouteListFinding(
            finding.Code,
            finding.Status,
            finding.CanonicalLogicalSubject,
            finding.Cause);
    }

    internal static IReadOnlyList<RouteListTopologyFindingAtDepth> ReadRelevantInventoryFindings(
        RouteListTopologyInput input,
        RouteListTopologyFacts topology,
        IReadOnlyList<RouteListTopologyNode> selectedRoots)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(selectedRoots);
        var relevant = new List<RouteListTopologyFindingAtDepth>();
        foreach (var finding in input.Inventory.Findings)
        {
            var depth = ReadRelevantDepth(input, topology, selectedRoots, finding);
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
        RouteListTopologyNode node,
        RouteListFilesystemFinding finding)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(finding);
        if (finding.Status == CliSemanticStatus.Attention
            || node.Source.Source.Kind != RouteListSourceKind.Entrypoint)
        {
            return false;
        }

        return ReadLexicalRelativeDepth(node, finding.CanonicalLogicalSubject) == 1;
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
        ArgumentNullException.ThrowIfNull(findings);
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
        ArgumentNullException.ThrowIfNull(finding);
        return finding.Subject is null
            ? finding.Cause
            : $"{finding.Subject}: {finding.Cause}";
    }

    private static int? ReadRelevantDepth(
        RouteListTopologyInput input,
        RouteListTopologyFacts topology,
        IReadOnlyList<RouteListTopologyNode> selectedRoots,
        RouteListFilesystemFinding finding)
    {
        if (string.Equals(finding.CanonicalLogicalSubject, RouteListLogicalPath.AgentsRoot, StringComparison.Ordinal))
        {
            return 0;
        }

        int? minimum = null;
        foreach (var root in selectedRoots)
        {
            var depth = ReadRelativeDepth(root, topology, finding.CanonicalLogicalSubject);
            if (depth is null || !IsRequested(input.Request.RequestedDepth, depth.Value))
            {
                continue;
            }

            minimum = minimum is null
                ? depth
                : Math.Min(minimum.Value, depth.Value);
        }

        return minimum;
    }

    private static int? ReadRelativeDepth(
        RouteListTopologyNode root,
        RouteListTopologyFacts topology,
        string subject)
    {
        var source = root.Source.Source;
        if (string.Equals(subject, source.CanonicalPath, StringComparison.Ordinal)
            || string.Equals(subject, source.OverwritePath, StringComparison.Ordinal))
        {
            return 0;
        }

        if (source.Kind != RouteListSourceKind.Entrypoint)
        {
            return null;
        }

        if (topology.TryReadRelativeDepth(source.CanonicalPath, subject, out var graphDepth))
        {
            return graphDepth;
        }

        var lexicalDepth = ReadLexicalRelativeDepth(root, subject);
        if (lexicalDepth is null
            || !HasRepresentedIntermediateChain(root, topology, subject, lexicalDepth.Value))
        {
            return null;
        }

        return lexicalDepth;
    }

    private static int? ReadLexicalRelativeDepth(
        RouteListTopologyNode root,
        string subject)
    {
        var sourcePath = ReadLogicalSourcePath(subject);
        var routeDirectory = RouteListTopologyFacts.ReadRouteDirectory(root);
        if (string.Equals(sourcePath, routeDirectory, StringComparison.Ordinal))
        {
            return 1;
        }

        var prefix = routeDirectory + "/";
        if (!sourcePath.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        var segments = sourcePath[prefix.Length..].Split('/', StringSplitOptions.None);
        var depth = segments.Length;
        var fileName = segments[^1];
        if (string.Equals(fileName, "SKILL.md", StringComparison.Ordinal)
            || RouteListSourceIdentity.IsRecognizedEntrypointPath(sourcePath))
        {
            depth--;
        }

        return Math.Max(1, depth);
    }

    private static bool IsRequested(RouteListDepth requestedDepth, int relativeDepth)
    {
        return requestedDepth.Kind == RouteListDepthKind.All
            || relativeDepth <= requestedDepth.Value!.Value;
    }

    private static string ReadLogicalSourcePath(string subject)
    {
        return subject.EndsWith(".overwrite.md", StringComparison.Ordinal)
            ? subject[..^".overwrite.md".Length] + ".md"
            : subject;
    }

    private static bool HasRepresentedIntermediateChain(
        RouteListTopologyNode root,
        RouteListTopologyFacts topology,
        string subject,
        int relativeDepth)
    {
        if (relativeDepth <= 1)
        {
            return true;
        }

        var routeDirectory = RouteListTopologyFacts.ReadRouteDirectory(root);
        var relative = subject[(routeDirectory.Length + 1)..];
        var segments = relative.Split('/', StringSplitOptions.None);
        var currentDirectory = routeDirectory;
        for (var index = 0; index < relativeDepth - 1; index++)
        {
            currentDirectory += "/" + segments[index];
            var represented = topology.Nodes.Any(node =>
                node.Source.Source.Kind == RouteListSourceKind.Entrypoint
                && string.Equals(
                    RouteListLogicalPath.ReadParent(node.Source.Source.CanonicalPath),
                    currentDirectory,
                    StringComparison.Ordinal));
            if (!represented)
            {
                return false;
            }
        }

        return true;
    }
}
