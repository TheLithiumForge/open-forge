using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal static class RepairOutsideAdmission
{
    private static readonly RoutedMetadataCompletionPolicy ProtectionPolicy = new();

    internal static bool AllowsNavigationInput(string path, RoutedMetadataPolicyContext policy, bool writesEntries, CancellationToken token)
    {
        if (!Allows(path, policy, token)
            || WorkspaceRemovals.IsPathRemoved(policy.Layout, path, policy.Settings)
            || !new SourceSharing(policy.Ownership.Framework?.GitIgnoredRoutes ?? []).Includes(path)) return false;
        var protection = ProtectionPolicy.Protection(path, policy);
        if (protection is not (RoutedMetadataEligibility.Eligible or RoutedMetadataEligibility.ManagedContent
            or RoutedMetadataEligibility.GeneratedContent or RoutedMetadataEligibility.LibraryProjection)) return false;
        return !writesEntries || !policy.Ownership.Libraries.Any(library => library.Paths.Contains(path, StringComparer.Ordinal));
    }

    internal static bool Allows(string path, RoutedMetadataPolicyContext policy, CancellationToken token, bool allowMissing = false)
    {
        token.ThrowIfCancellationRequested();
        if (!policy.Layout.TryGetMaintenanceBoundary(path, out var boundary) || boundary.Kind != WorkspaceMaintenanceBoundaryKind.Outside)
            return true;
        if (policy.AvailableOutsideBoundary(boundary)?.PhysicalDirectory is not { } physicalBoundary) return false;
        var workspace = policy.Layout.Workspace;
        var paths = new PhysicalPathResolver();
        var lexicalBoundary = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, boundary.DirectoryPath);
        var current = paths.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, lexicalBoundary);
        if (current.State != PhysicalPathState.Contained
            || !PhysicalIdentityTracker.PathComparer.Equals(current.ResolvedPhysicalPath, physicalBoundary)) return false;
        var lexical = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path);
        var resolved = paths.ResolveCandidate(lexicalBoundary, physicalBoundary, lexical);
        if (resolved.State is not (PhysicalPathState.Contained or PhysicalPathState.Missing)) return false;
        var expected = Path.GetFullPath(Path.Combine(physicalBoundary, Path.GetRelativePath(lexicalBoundary, lexical)));
        if (resolved.State == PhysicalPathState.Contained
            && !PhysicalIdentityTracker.PathComparer.Equals(resolved.ResolvedPhysicalPath, expected)) return false;
        var leaf = NoFollowLeafObserver.Observe(paths, workspace, lexical, token);
        return leaf.State == NoFollowLeafState.OrdinaryFile || allowMissing && leaf.State == NoFollowLeafState.Missing;
    }
}
