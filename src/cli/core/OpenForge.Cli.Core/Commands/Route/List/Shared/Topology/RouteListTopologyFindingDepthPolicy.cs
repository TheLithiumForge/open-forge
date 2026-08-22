using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal static class RouteListTopologyFindingDepthPolicy
{
    internal static int? ReadRelevantDepth(
        RouteListTopologyInput input,
        RouteTopologyFacts topology,
        IReadOnlyList<RouteTopologyNode> selectedRoots,
        RouteListFilesystemFinding finding)
    {
        if (string.Equals(finding.CanonicalLogicalSubject, RouteLogicalPath.AgentsRoot, StringComparison.Ordinal))
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

    internal static int? ReadLexicalRelativeDepth(
        RouteTopologyNode root,
        string subject)
    {
        var sourcePath = ReadLogicalSourcePath(subject);
        var routeDirectory = RouteTopologyFacts.ReadRouteDirectory(root);
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
            || RouteSourceIdentity.IsRecognizedEntrypointPath(sourcePath))
        {
            depth--;
        }

        return Math.Max(1, depth);
    }

    private static int? ReadRelativeDepth(
        RouteTopologyNode root,
        RouteTopologyFacts topology,
        string subject)
    {
        var source = root.Source;
        if (string.Equals(subject, source.CanonicalPath, StringComparison.Ordinal)
            || string.Equals(subject, source.OverwritePath, StringComparison.Ordinal))
        {
            return 0;
        }

        if (source.Kind != RouteSourceKind.Entrypoint)
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
        RouteTopologyNode root,
        RouteTopologyFacts topology,
        string subject,
        int relativeDepth)
    {
        if (relativeDepth <= 1)
        {
            return true;
        }

        var routeDirectory = RouteTopologyFacts.ReadRouteDirectory(root);
        var relative = subject[(routeDirectory.Length + 1)..];
        var segments = relative.Split('/', StringSplitOptions.None);
        var currentDirectory = routeDirectory;
        for (var index = 0; index < relativeDepth - 1; index++)
        {
            currentDirectory += "/" + segments[index];
            var represented = topology.Nodes.Any(node =>
                node.Source.Kind == RouteSourceKind.Entrypoint
                && string.Equals(
                    RouteLogicalPath.ReadParent(node.Source.CanonicalPath),
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
