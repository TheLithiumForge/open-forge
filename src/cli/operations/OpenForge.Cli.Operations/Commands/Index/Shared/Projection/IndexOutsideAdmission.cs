using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Projection;

internal sealed class IndexOutsideAdmission(SourceCatalogue catalogue, RoutedMetadataPolicyContext policy)
{
    private static readonly RoutedMetadataCompletionPolicy ProtectionPolicy = new();

    internal bool Allows(SourceLogicalSource source, IndexProjectionDocumentRole role, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var path = source.Identity.CanonicalBasePath;
        if (!catalogue.Layout.TryGetMaintenanceBoundary(path, out var boundary) || boundary.Kind != WorkspaceMaintenanceBoundaryKind.Outside
            || !ReferenceEquals(catalogue.Layout, policy.Layout)
            || policy.AvailableOutsideBoundary(boundary)?.PhysicalDirectory is not { } physicalBoundary) return false;
        if (WorkspaceRemovals.IsPathRemoved(policy.Layout, path, policy.Settings)
            || !new SourceSharing(policy.Ownership.Framework?.GitIgnoredRoutes ?? []).Includes(path)) return false;
        var protection = ProtectionPolicy.Protection(path, policy);
        if (protection is not (RoutedMetadataEligibility.Eligible or RoutedMetadataEligibility.ManagedContent
            or RoutedMetadataEligibility.GeneratedContent or RoutedMetadataEligibility.LibraryProjection)) return false;
        if (role == IndexProjectionDocumentRole.Target
            && policy.Ownership.Libraries.Any(library => library.Paths.Contains(path, StringComparer.Ordinal))) return false;
        var workspace = catalogue.Workspace;
        var paths = new PhysicalPathResolver();
        var lexicalBoundary = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, boundary.DirectoryPath);
        var current = paths.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, lexicalBoundary);
        if (current.State != PhysicalPathState.Contained
            || !PhysicalIdentityTracker.PathComparer.Equals(current.ResolvedPhysicalPath, physicalBoundary)) return false;
        var lexical = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path);
        var target = paths.ResolveCandidate(lexicalBoundary, physicalBoundary, lexical);
        var expected = Path.GetFullPath(Path.Combine(physicalBoundary, Path.GetRelativePath(lexicalBoundary, lexical)));
        var candidate = catalogue.FindCandidateByPath(path);
        if (target.State != PhysicalPathState.Contained || candidate?.PhysicalState != PhysicalPathState.Contained
            || !PhysicalIdentityTracker.PathComparer.Equals(target.ResolvedPhysicalPath, expected)
            || !PhysicalIdentityTracker.PathComparer.Equals(candidate.PhysicalPath, expected)
            || !PhysicalIdentityTracker.PathComparer.Equals(candidate.PhysicalParentPath, Path.GetDirectoryName(expected))
            || !PhysicalIdentityTracker.PathComparer.Equals(source.Base.PhysicalPath, expected)
            || catalogue.Aliases.Any(alias => alias.Key == path || alias.Value == path)
            || catalogue.Candidates.Any(item => item.CanonicalPath != path && item.PhysicalPath is { } physical
                && PhysicalIdentityTracker.PathComparer.Equals(physical, expected))
            || catalogue.FindLayersByPhysicalPath(expected).Any(item => item.Layer.CanonicalPath != path)) return false;
        return NoFollowLeafObserver.Observe(paths, workspace, lexical, token).State == NoFollowLeafState.OrdinaryFile;
    }
}
