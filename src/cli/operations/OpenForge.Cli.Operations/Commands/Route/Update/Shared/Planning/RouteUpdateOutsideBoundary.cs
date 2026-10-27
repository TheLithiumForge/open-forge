using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal static class RouteUpdateOutsideBoundary
{
    internal static WorkspaceMaintenanceBoundary? Find(WorkspaceLayoutRead observation, string path)
        => observation.Layout is { } layout
            && layout.TryGetMaintenanceBoundary(path, out var boundary)
            && boundary.Kind == WorkspaceMaintenanceBoundaryKind.Outside ? boundary : null;

    internal static RouteUpdateFinding? ReadBoundary(
        WorkspaceLayoutRead observation, string path, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return new(RouteUpdateFindingCode.Interrupted, "Outside source observation was interrupted.", path);
        if (Find(observation, path) is not { } boundary) return null;
        var outside = observation.OutsideRoutes.SingleOrDefault(item =>
            item.Descriptor.DirectoryPath == boundary.DirectoryPath && item.Descriptor.CataloguePath == boundary.AnchorPath);
        if (outside is not { State: WorkspaceOutsideRouteReadState.Available, PhysicalDirectory: { } physicalBoundary })
            return new(outside?.State == WorkspaceOutsideRouteReadState.Unavailable
                ? RouteUpdateFindingCode.InspectionIncomplete : RouteUpdateFindingCode.TargetUnsafe,
                "The registered outside folder is unavailable for Route Update.", path);

        var workspace = observation.Layout?.Workspace
            ?? throw new InvalidOperationException("An outside boundary requires its observed layout.");
        var paths = new PhysicalPathResolver().CreateReadSession();
        var root = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, boundary.DirectoryPath);
        var currentRoot = paths.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, root);
        if (currentRoot.State != PhysicalPathState.Contained
            || !PhysicalIdentityTracker.PathComparer.Equals(currentRoot.ResolvedPhysicalPath, physicalBoundary))
            return new(RouteUpdateFindingCode.TargetUnsafe, "The registered outside folder changed its physical location.", path);

        var target = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path);
        var current = target;
        while (true)
        {
            if (token.IsCancellationRequested)
                return new(RouteUpdateFindingCode.Interrupted, "Outside source observation was interrupted.", path);
            var leaf = NoFollowLeafObserver.Observe(paths, workspace, current, token);
            var required = current == target ? NoFollowLeafState.OrdinaryFile : NoFollowLeafState.Directory;
            if (current == target && leaf.State is NoFollowLeafState.Missing or NoFollowLeafState.Directory)
                return new(RouteUpdateFindingCode.InvalidTarget, "Route Update requires an existing Markdown file.", path);
            if (leaf.State is NoFollowLeafState.Inaccessible)
                return new(RouteUpdateFindingCode.InspectionIncomplete, "An outside source path could not be read.", path);
            var resolved = paths.ResolveCandidate(root, physicalBoundary, current);
            var expected = Path.GetFullPath(Path.Combine(physicalBoundary, Path.GetRelativePath(root, current)));
            if (leaf.State != required || resolved.State != PhysicalPathState.Contained
                || !PhysicalIdentityTracker.PathComparer.Equals(resolved.ResolvedPhysicalPath, expected))
                return new(RouteUpdateFindingCode.TargetUnsafe, "The outside source is not an ordinary file inside its registered folder.", path);
            if (current == root) return null;
            current = Path.GetDirectoryName(current)
                ?? throw new InvalidOperationException("An outside source requires bounded ancestry.");
        }
    }

    internal static RouteUpdateFinding? WriteBoundary(
        WorkspaceLayoutRead observation, string path, bool entries, CancellationToken token)
    {
        if (Find(observation, path) is null) return null;
        if (ReadBoundary(observation, path, token) is { } physical) return physical;
        var policy = ReferenceProtectionReader.Read(observation).Policy;
        if (policy is null)
            return new(RouteUpdateFindingCode.TargetUnsafe, "Outside source protection could not be established.", path);
        if (WorkspaceRemovals.IsPathRemoved(policy.Layout, path, policy.Settings)
            || !new SourceSharing(policy.Ownership.Framework?.GitIgnoredRoutes ?? []).Includes(path))
            return new(RouteUpdateFindingCode.TargetUnsafe, "The outside update is excluded by workspace policy.", path);
        var protection = new RoutedMetadataCompletionPolicy().Protection(path, policy);
        var boundedEntries = entries && protection is RoutedMetadataEligibility.ManagedContent or RoutedMetadataEligibility.GeneratedContent;
        if (protection != RoutedMetadataEligibility.Eligible && !boundedEntries
            || policy.Ownership.Libraries.Any(library => library.Paths.Contains(path, StringComparer.Ordinal)))
            return new(RouteUpdateFindingCode.TargetUnsafe, "The outside update is protected by workspace policy.", path);
        return null;
    }
}
