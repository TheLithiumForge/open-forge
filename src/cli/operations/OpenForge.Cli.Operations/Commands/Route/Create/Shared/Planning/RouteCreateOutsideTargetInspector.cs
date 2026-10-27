using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;

internal static class RouteCreateOutsideTargetInspector
{
    internal static RouteCreateFinding? Availability(WorkspaceLayoutRead observation, string path)
    {
        if (observation.Layout is not { } layout || !layout.TryGetMaintenanceBoundary(path, out var boundary)
            || boundary.Kind != WorkspaceMaintenanceBoundaryKind.Outside) return null;
        var outside = observation.OutsideRoutes.SingleOrDefault(route => route.Descriptor.DirectoryPath == boundary.DirectoryPath
            && route.Descriptor.CataloguePath == boundary.AnchorPath);
        if (outside is { State: WorkspaceOutsideRouteReadState.Available, PhysicalDirectory: not null }) return null;
        return new(outside?.State == WorkspaceOutsideRouteReadState.Unavailable
                ? RouteCreateFindingCode.InspectionIncomplete : RouteCreateFindingCode.TargetUnsafe,
            outside?.Failure?.DirectCause ?? "The selected outside route directory is unavailable for Route Create.", path);
    }

    internal static RouteCreateFinding? Inspect(WorkspaceLayoutRead observation, SourceCatalogue catalogue,
        string path, NoFollowLeafState ordinaryKind, CancellationToken token)
    {
        var layout = catalogue.Layout;
        if (!layout.TryGetMaintenanceBoundary(path, out var boundary) || boundary.Kind != WorkspaceMaintenanceBoundaryKind.Outside)
            return null;
        if (!ReferenceEquals(observation.Layout, layout)) return Unsafe(path);
        if (Availability(observation, path) is { } unavailable) return unavailable;
        var outside = observation.OutsideRoutes.Single(route => route.Descriptor.DirectoryPath == boundary.DirectoryPath);
        if (outside.PhysicalDirectory is not { } physicalBoundary) return Unsafe(path);
        var policy = ReferenceProtectionReader.Read(observation).Policy;
        var protectedPath = ordinaryKind == NoFollowLeafState.Directory ? WorkspaceRoutePath.CanonicalEntrypointPath(path) : path;
        if (policy is null || !AllowsEffect(protectedPath, catalogue, policy))
            return new(RouteCreateFindingCode.TargetUnsafe, "The selected outside Route Create effect is protected by workspace policy.", path);

        var workspace = layout.Workspace;
        var paths = new PhysicalPathResolver().CreateReadSession();
        var lexicalBoundary = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, boundary.DirectoryPath);
        var currentBoundary = paths.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, lexicalBoundary);
        if (currentBoundary.State != PhysicalPathState.Contained
            || !PhysicalIdentityTracker.PathComparer.Equals(currentBoundary.ResolvedPhysicalPath, physicalBoundary)) return Unsafe(path);
        var lexicalPath = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path);
        var expectedPath = Path.GetFullPath(Path.Combine(physicalBoundary, Path.GetRelativePath(lexicalBoundary, lexicalPath)));
        var current = lexicalPath;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            var leaf = NoFollowLeafObserver.Observe(paths, workspace, current, token);
            var required = current == lexicalPath ? ordinaryKind : NoFollowLeafState.Directory;
            if (leaf.State != NoFollowLeafState.Missing && leaf.State != required) return Unsafe(path);
            var resolved = paths.ResolveCandidate(lexicalBoundary, physicalBoundary, current);
            var expected = Path.GetFullPath(Path.Combine(physicalBoundary, Path.GetRelativePath(lexicalBoundary, current)));
            if (resolved.State is not (PhysicalPathState.Contained or PhysicalPathState.Missing)
                || resolved.State == PhysicalPathState.Contained
                    && !PhysicalIdentityTracker.PathComparer.Equals(resolved.ResolvedPhysicalPath, expected)) return Unsafe(path);
            if (current == lexicalBoundary) break;
            current = Path.GetDirectoryName(current) ?? throw new InvalidOperationException("An outside target requires bounded ancestry.");
        }

        if (catalogue.Aliases.Any(alias => alias.Key == path || alias.Value == path)
            || catalogue.Candidates.Any(candidate => candidate.CanonicalPath != path && candidate.PhysicalPath is { } physical
                && PhysicalIdentityTracker.PathComparer.Equals(physical, expectedPath))
            || catalogue.FindLayersByPhysicalPath(expectedPath).Any(layer => layer.Layer.CanonicalPath != path)) return Unsafe(path);
        var source = catalogue.FindByPath(path);
        if (source is not null && (source.Base.CanonicalPath != path
            || !PhysicalIdentityTracker.PathComparer.Equals(source.Base.PhysicalPath, expectedPath))) return Unsafe(path);
        return null;
    }

    private static bool AllowsEffect(string path, SourceCatalogue catalogue, RoutedMetadataPolicyContext policy)
    {
        if (WorkspaceRemovals.IsPathRemoved(policy.Layout, path, policy.Settings)
            || !new SourceSharing(policy.Ownership.Framework?.GitIgnoredRoutes ?? []).Includes(path)
            || policy.Ownership.Libraries.Any(library => library.Paths.Contains(path, StringComparer.Ordinal))) return false;
        var protection = new RoutedMetadataCompletionPolicy().Protection(path, policy);
        if (protection == RoutedMetadataEligibility.Eligible) return true;
        return protection is RoutedMetadataEligibility.ManagedContent or RoutedMetadataEligibility.GeneratedContent
            && catalogue.FindByPath(path) is { } source && SourceFormClassifier.IsEntrypoint(source.Base.Form);
    }

    private static RouteCreateFinding Unsafe(string path)
        => new(RouteCreateFindingCode.TargetUnsafe, "The outside Route Create target is not an ordinary leaf at its retained physical coordinate.", path);
}
