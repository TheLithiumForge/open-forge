using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal static class RouteRemoveOutsideBoundaryInspector
{
    internal static WorkspaceOutsideRoute? Find(WorkspaceLayout layout, string path)
        => layout.OutsideRoutes.SingleOrDefault(route => Contains(route.DirectoryPath, path));

    internal static bool Contains(string directory, string path)
        => path == directory || path.StartsWith(directory + "/", StringComparison.Ordinal);

    internal static bool IsProtected(WorkspaceLayout layout, string path)
    {
        var canonical = SourceOverwritePath.HasSuffix(path) ? SourceOverwritePath.ReadBasePath(path) : path;
        return layout.OutsideRoutes.Any(route => Contains(canonical, route.DirectoryPath)
            || SourceLogicalPath.ReadParent(canonical) == route.DirectoryPath
                && SourceIdentity.IsRecognizedEntrypointPath(layout, canonical));
    }

    internal static bool IsAlias(WorkspaceLayout layout, string path)
        => layout.OutsideRoutes.Any(route =>
            (path.Equals(route.DirectoryPath, StringComparison.OrdinalIgnoreCase)
                || path.StartsWith(route.DirectoryPath + "/", StringComparison.OrdinalIgnoreCase)
                || route.DirectoryPath.StartsWith(path + "/", StringComparison.OrdinalIgnoreCase))
            && !Contains(route.DirectoryPath, path) && !Contains(path, route.DirectoryPath));

    internal static string? Validate(WorkspaceLayoutRead observation, string path, CancellationToken cancellationToken,
        bool allowMissing = false)
    {
        var layout = observation.Layout ?? throw new InvalidOperationException("Outside removal requires its retained layout.");
        var route = Find(layout, path);
        if (route is null) return "The target does not belong to the selected registered outside boundary.";
        var boundary = observation.OutsideRoutes.SingleOrDefault(read => read.Descriptor == route);
        if (boundary is not { State: WorkspaceOutsideRouteReadState.Available, PhysicalDirectory: { } physicalDirectory })
            return "The selected registered outside directory is not available.";
        var resolver = new PhysicalPathResolver();
        var segments = path.Split('/');
        var logical = string.Empty;
        for (var index = 0; index < segments.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logical = logical.Length == 0 ? segments[index] : logical + "/" + segments[index];
            var lexical = SourceLogicalPath.ToLexicalPath(layout.Workspace.LexicalRoot, logical);
            var leaf = NoFollowLeafObserver.Observe(resolver, layout.Workspace, lexical, cancellationToken);
            if (leaf.State == NoFollowLeafState.Missing)
                return allowMissing && Contains(route.DirectoryPath, logical) && logical != route.DirectoryPath
                    ? null : "An outside removal path or its registered directory is missing.";
            if (index < segments.Length - 1 || logical == route.DirectoryPath ? leaf.State != NoFollowLeafState.Directory
                : leaf.State is not (NoFollowLeafState.OrdinaryFile or NoFollowLeafState.Directory))
                return "Outside removal requires ordinary no-follow directories and leaves.";
            var resolution = resolver.ResolveCandidate(layout.Workspace.LexicalRoot, layout.Workspace.PhysicalRoot, lexical);
            if (resolution.State != PhysicalPathState.Contained)
                return "The outside removal coordinate cannot be resolved safely.";
            if (Contains(route.DirectoryPath, logical))
            {
                var suffix = logical == route.DirectoryPath ? string.Empty : logical[(route.DirectoryPath.Length + 1)..];
                var expected = Path.GetFullPath(Path.Combine(physicalDirectory, suffix.Replace('/', Path.DirectorySeparatorChar)));
                if (!PhysicalIdentityTracker.PathComparer.Equals(expected, resolution.GetContainedPhysicalPath()))
                    return "The outside removal path changed its retained physical coordinate.";
            }
        }
        return null;
    }

    internal static bool HasParentChain(SourceCatalogue catalogue, string path, bool category,
        WorkspaceLayoutRead observation, CancellationToken cancellationToken)
    {
        var route = Find(catalogue.Layout, path);
        if (route is null) return false;
        var directory = SourceLogicalPath.ReadParent(path);
        if (category) directory = SourceLogicalPath.ReadParent(directory);
        while (Contains(route.DirectoryPath, directory))
        {
            var entries = catalogue.Sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form)
                && SourceLogicalPath.ReadParent(source.Base.CanonicalPath) == directory).ToArray();
            if (entries.Length != 1 || entries[0].Identity.CanonicalBasePath != entries[0].Base.CanonicalPath
                || Validate(observation, entries[0].Base.CanonicalPath, cancellationToken) is not null
                || catalogue.Aliases.Any(alias => alias.Key == entries[0].Base.CanonicalPath && alias.Value != alias.Key))
                return false;
            if (directory == route.DirectoryPath) return true;
            directory = SourceLogicalPath.ReadParent(directory);
        }
        return false;
    }

    internal static bool IsRequiredParent(SourceCatalogue catalogue, string selectedPath, bool category, string parentPath)
    {
        var route = Find(catalogue.Layout, selectedPath);
        if (route is null || !Contains(route.DirectoryPath, parentPath)
            || !SourceIdentity.IsRecognizedEntrypointPath(catalogue.Layout, parentPath)) return false;
        var directory = SourceLogicalPath.ReadParent(selectedPath);
        if (category) directory = SourceLogicalPath.ReadParent(directory);
        return Contains(SourceLogicalPath.ReadParent(parentPath), directory);
    }

    internal static string? ValidateCurrentAnchors(RouteRemovePlan plan, CancellationToken cancellationToken)
    {
        var subject = plan.Projection.Subject;
        if (subject.OutsideRoute is not { } boundary) return null;
        if (Validate(plan.LayoutRead, boundary.DirectoryPath, cancellationToken) is { } rootCause) return rootCause;
        var changed = plan.Projection.FileChanges;
        var deleted = changed.Where(change => change.Kind == PlannedFileChangeKind.Delete)
            .Select(change => Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, change.LogicalPath).Replace('\\', '/'))
            .ToHashSet(StringComparer.Ordinal);
        var required = changed.Where(change => change.Kind != PlannedFileChangeKind.Delete)
            .Select(change => Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, change.LogicalPath).Replace('\\', '/'))
            .Append(subject.SelectedSource.Base.CanonicalPath).ToArray();
        var anchors = subject.Catalogue.Sources.Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form)
                && Contains(boundary.DirectoryPath, source.Base.CanonicalPath) && !deleted.Contains(source.Base.CanonicalPath)
                && required.Any(path => Contains(SourceLogicalPath.ReadParent(source.Base.CanonicalPath), path)))
            .GroupBy(source => SourceLogicalPath.ReadParent(source.Base.CanonicalPath), StringComparer.Ordinal).ToArray();
        if (anchors.Length == 0) return "Outside removal requires its retained structural anchors.";
        var resolver = new PhysicalPathResolver();
        foreach (var group in anchors)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (group.Count() != 1) return "A required retained outside structural parent is ambiguous.";
            var retained = group.Single().Base.CanonicalPath;
            if (Validate(plan.LayoutRead, retained, cancellationToken) is { } cause) return cause;
            try
            {
                var directory = SourceLogicalPath.ToLexicalPath(plan.Request.Workspace.LexicalRoot, group.Key);
                var current = Directory.EnumerateFileSystemEntries(directory)
                    .Select(path => group.Key + "/" + Path.GetFileName(path))
                    .Where(path => SourceIdentity.IsRecognizedEntrypointPath(plan.Layout, path)).ToArray();
                if (current.Length != 1 || current[0] != retained)
                    return "A required outside structural anchor changed or acquired a competing entrypoint.";
                var leaf = NoFollowLeafObserver.Observe(resolver, plan.Request.Workspace,
                    SourceLogicalPath.ToLexicalPath(plan.Request.Workspace.LexicalRoot, retained), cancellationToken);
                if (leaf.State != NoFollowLeafState.OrdinaryFile)
                    return "A retained outside structural anchor is no longer an ordinary no-follow file.";
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return "A required outside structural parent could not be inspected completely.";
            }
        }
        return null;
    }

    internal static string? ValidateEffect(RouteRemoveResolvedSubject subject, string path, CancellationToken cancellationToken)
    {
        if (subject.OutsideRoute is not { } boundary) return null;
        if (!Contains(boundary.DirectoryPath, path))
            return "A required surviving content change leaves the selected outside registration.";
        return Validate(subject.LayoutRead ?? throw new InvalidOperationException("Outside removal requires its observation."),
            path, cancellationToken);
    }
}
