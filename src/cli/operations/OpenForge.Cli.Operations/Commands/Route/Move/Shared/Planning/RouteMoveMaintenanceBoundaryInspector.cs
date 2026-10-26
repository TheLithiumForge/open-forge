using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal static class RouteMoveMaintenanceBoundaryInspector
{
    internal static bool IsOutside(SourceCatalogue catalogue, string path)
        => SourceLogicalPath.IsOutsideSource(catalogue.Layout, path);

    internal static WorkspaceOutsideRouteRead? Observation(WorkspaceLayoutRead read, string path)
    {
        if (read.Layout is not { } layout || !layout.TryGetMaintenanceBoundary(path, out var boundary)
            || boundary.Kind != WorkspaceMaintenanceBoundaryKind.Outside) return null;
        return read.OutsideRoutes.SingleOrDefault(route => route.Descriptor.DirectoryPath == boundary.DirectoryPath
            && route.Descriptor.CataloguePath == boundary.AnchorPath);
    }

    internal static bool SameBoundary(WorkspaceLayout layout, string source, string destination)
    {
        var hasSource = layout.TryGetMaintenanceBoundary(source, out var first);
        var hasDestination = layout.TryGetMaintenanceBoundary(destination, out var second);
        if (hasSource && first.Kind != WorkspaceMaintenanceBoundaryKind.Outside
            && hasDestination && second.Kind != WorkspaceMaintenanceBoundaryKind.Outside) return true;
        return hasSource && hasDestination && first.Kind == WorkspaceMaintenanceBoundaryKind.Outside
            && second.Kind == WorkspaceMaintenanceBoundaryKind.Outside
            && string.Equals(first.DirectoryPath, second.DirectoryPath, StringComparison.Ordinal)
            && string.Equals(first.AnchorPath, second.AnchorPath, StringComparison.Ordinal);
    }

    internal static string? ReadChainProblem(WorkspaceLayoutRead read, SourceCatalogue catalogue, SourceRouteTopology topology, string path, bool allowAnchor = false)
    {
        var observation = Observation(read, path);
        if (observation is not { State: WorkspaceOutsideRouteReadState.Available, PhysicalDirectory: not null })
            return "The selected outside registration is unavailable or unsafe.";
        var root = observation.Descriptor.DirectoryPath;
        var anchors = catalogue.Sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form)
            && SourceLogicalPath.ReadParent(source.Base.CanonicalPath) == root).ToArray();
        if (anchors.Length != 1) return "The outside registration requires one recognized anchor.";
        var anchor = anchors[0].Base.CanonicalPath;
        if (!allowAnchor && path == anchor) return "A registered outside anchor cannot be moved.";
        var current = path;
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (current != anchor)
        {
            if (!visited.Add(current) || !current.StartsWith(root + "/", StringComparison.Ordinal))
                return "The outside source does not retain one bounded structural ancestry.";
            var node = topology.FindByPath(current);
            if (node?.ParentState != SourceRouteParentState.Resolved || node.ParentPaths.Count != 1)
                return "The outside source requires one existing structural parent.";
            current = node.ParentPaths[0];
        }
        return ReadPathProblem(read, path, allowMissing: false);
    }

    internal static string? ReadPathProblem(WorkspaceLayoutRead read, string path, bool allowMissing)
    {
        var observation = Observation(read, path);
        if (observation is not { State: WorkspaceOutsideRouteReadState.Available, PhysicalDirectory: { } physicalRoot })
            return "The selected outside registration is unavailable or unsafe.";
        var layout = read.Layout ?? throw new InvalidOperationException("Outside admission requires its retained layout.");
        var workspace = layout.Workspace;
        var root = observation.Descriptor.DirectoryPath;
        var logicalRoot = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, root);
        var logical = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path);
        var resolver = new PhysicalPathResolver();
        if (NoFollowLeafObserver.Observe(resolver, workspace, logicalRoot, CancellationToken.None).State != NoFollowLeafState.Directory)
            return "The outside registration is no longer an ordinary directory.";
        var rootResolution = resolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, logicalRoot);
        if (rootResolution.State != PhysicalPathState.Contained
            || !string.Equals(rootResolution.GetContainedPhysicalPath(), physicalRoot, StringComparison.Ordinal))
            return "The outside registration's physical mapping changed.";
        try
        {
            var anchors = Directory.EnumerateFileSystemEntries(physicalRoot)
                .Where(entry => SourceIdentity.IsRecognizedEntrypointPath(layout, root + "/" + Path.GetFileName(entry))).ToArray();
            if (anchors.Length != 1 || NoFollowLeafObserver.Observe(resolver, workspace,
                    Path.Combine(logicalRoot, Path.GetFileName(anchors[0])), CancellationToken.None).State != NoFollowLeafState.OrdinaryFile)
                return "The outside registration no longer has one ordinary recognized anchor.";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return "The outside registration's anchor is unavailable.";
        }
        var relative = path == root ? string.Empty : path[(root.Length + 1)..];
        var current = logicalRoot;
        var physicalParent = physicalRoot;
        foreach (var segment in relative.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var names = Directory.EnumerateFileSystemEntries(physicalParent).Select(Path.GetFileName).ToArray();
                if (!names.Contains(segment, StringComparer.Ordinal)
                    && names.Contains(segment, StringComparer.OrdinalIgnoreCase)) return "An outside path uses a case alias.";
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return "An outside path's physical parent is unavailable.";
            }
            current = Path.Combine(current, segment);
            var leaf = NoFollowLeafObserver.Observe(resolver, workspace, current, CancellationToken.None);
            if (leaf.State == NoFollowLeafState.Missing) return allowMissing ? null : "An outside input is missing.";
            if (leaf.State is not (NoFollowLeafState.Directory or NoFollowLeafState.OrdinaryFile))
                return "An outside path crosses a link or special filesystem leaf.";
            physicalParent = Path.Combine(physicalParent, segment);
        }
        var resolution = resolver.ResolveCandidate(logicalRoot, physicalRoot, logical);
        var expected = Path.Combine(physicalRoot, relative.Replace('/', Path.DirectorySeparatorChar));
        return resolution.State == PhysicalPathState.Contained
            && string.Equals(resolution.GetContainedPhysicalPath(), expected, StringComparison.Ordinal)
            ? null : "An outside path does not retain its exact registered physical coordinate.";
    }
}
