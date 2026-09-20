using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal static class RouteListTopologyFindingDepthPolicy
{
    internal static int? ReadRelevantDepth(
        RouteListTopologyInput input,
        SourceRouteTopology topology,
        IReadOnlyList<SourceRouteNode> selectedRoots,
        RouteListFilesystemFinding finding)
    {
        if (string.Equals(finding.CanonicalLogicalSubject, SourceLogicalPath.AgentsRoot, StringComparison.Ordinal))
        {
            return 0;
        }

        int? minimum = null;
        foreach (var root in selectedRoots)
        {
            var depth = ReadRelativeDepth(root, topology, input, finding.CanonicalLogicalSubject);
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

    internal static int? ReadLexicalRelativeDepth(
        SourceRouteNode root,
        string subject)
    {
        var sourcePath = ReadLogicalSourcePath(subject);
        var routeDirectory = SourceLogicalPath.ReadParent(root.Identity.CanonicalBasePath);
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
            || SourceIdentity.IsRecognizedEntrypointPath(sourcePath))
        {
            depth--;
        }

        return Math.Max(1, depth);
    }

    private static int? ReadRelativeDepth(
        SourceRouteNode root,
        SourceRouteTopology topology,
        RouteListTopologyInput input,
        string subject)
    {
        var projection = RouteListTopologyProjectionPolicy.ReadProjection(input, root);
        if (string.Equals(subject, root.Identity.CanonicalBasePath, StringComparison.Ordinal)
            || string.Equals(subject, projection.LogicalSource.Overwrite?.CanonicalPath, StringComparison.Ordinal))
        {
            return 0;
        }

        if (!SourceFormClassifier.IsEntrypoint(projection.LogicalSource.Base.Form))
        {
            return null;
        }

        if (topology.TryReadRelativeDepth(root.Identity.CanonicalBasePath, subject, out var graphDepth))
        {
            return graphDepth;
        }

        var lexicalDepth = ReadLexicalRelativeDepth(root, subject);
        if (lexicalDepth is null
            || !HasRepresentedIntermediateChain(root, topology, input, subject, lexicalDepth.Value))
        {
            return null;
        }

        return lexicalDepth;
    }

    private static bool IsRequested(RouteListDepth requestedDepth, int relativeDepth)
    {
        return requestedDepth.Kind == RouteListDepthKind.All
            || relativeDepth <= requestedDepth.FiniteValue;
    }

    private static string ReadLogicalSourcePath(string subject)
    {
        return subject.EndsWith(".overwrite.md", StringComparison.Ordinal)
            ? subject[..^".overwrite.md".Length] + ".md"
            : subject;
    }

    private static bool HasRepresentedIntermediateChain(
        SourceRouteNode root,
        SourceRouteTopology topology,
        RouteListTopologyInput input,
        string subject,
        int relativeDepth)
    {
        if (relativeDepth <= 1)
        {
            return true;
        }

        var routeDirectory = SourceLogicalPath.ReadParent(root.Identity.CanonicalBasePath);
        var relative = subject[(routeDirectory.Length + 1)..];
        var segments = relative.Split('/', StringSplitOptions.None);
        var currentDirectory = routeDirectory;
        for (var index = 0; index < relativeDepth - 1; index++)
        {
            currentDirectory += "/" + segments[index];
            var represented = topology.Nodes.Any(node =>
                SourceFormClassifier.IsEntrypoint(
                    RouteListTopologyProjectionPolicy.ReadProjection(input, node).LogicalSource.Base.Form)
                && string.Equals(
                    SourceLogicalPath.ReadParent(node.Identity.CanonicalBasePath),
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
