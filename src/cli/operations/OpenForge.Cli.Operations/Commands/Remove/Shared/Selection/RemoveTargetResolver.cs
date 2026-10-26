using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Commands.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.Commands.Remove.Shared.Selection;

internal static class RemoveTargetResolver
{
    internal static async ValueTask<RemoveTargetResolution> ResolveAsync(
        CliWorkspace workspace,
        RemoveSelection selection,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(selection);
        var isPathSelection = selection.State == RemoveSelectionState.Path;
        if (selection.State is RemoveSelectionState.Path or RemoveSelectionState.Route
            && PortableWorkspacePath.TryNormalize(selection.Target, out var candidate))
        {
            var lexical = Path.GetFullPath(candidate, workspace.PhysicalRoot);
            var runtimeResolver = new PhysicalPathResolver();
            var current = Path.GetFullPath(candidate, workspace.LexicalRoot);
            while (current is not null)
            {
                var physical = runtimeResolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, current);
                if (WorkspaceRuntimePaths.Contains(workspace, lexical)
                    || (physical.ResolvedPhysicalPath is { } resolved && WorkspaceRuntimePaths.Contains(workspace, resolved)))
                {
                    return new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, selection.Target,
                        "The selected target is inside the reserved workspace runtime directory.");
                }
                if (physical.State != PhysicalPathState.Missing)
                {
                    break;
                }
                current = Path.GetDirectoryName(current);
            }
        }
        if (selection.State is RemoveSelectionState.Extension or RemoveSelectionState.Library)
        {
            var explicitKind = selection.State switch
            {
                RemoveSelectionState.Route => RemoveKind.Route,
                RemoveSelectionState.Extension => RemoveKind.Extension,
                RemoveSelectionState.Library => RemoveKind.Library,
                _ => throw new ArgumentOutOfRangeException(nameof(selection), selection.State, "The Remove selection state is not dispatchable."),
            };
            return new RemoveTargetResolution(explicitKind, selection.Target, null);
        }

        var layoutRead = await new WorkspaceLayoutReader(new PhysicalPathResolver())
            .ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        if (layoutRead.Layout is not { } layout)
        {
            if (isPathSelection) return new RemoveTargetResolution(RemoveKind.Path, selection.Target, null);
            return new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, selection.Target,
                layoutRead.Settings.Cause ?? "The workspace layout is unavailable, so this target cannot be classified safely.");
        }
        var target = selection.Target;
        var reference = SourceReferenceParser.Parse(target);
        if (!isPathSelection && reference.Kind != SourceReferenceKind.SourcePath && !layout.IsImplicitContentDestination(target))
            return new RemoveTargetResolution(RemoveKind.Route, target, null);
        var outsidePath = reference.Kind == SourceReferenceKind.SourcePath ? reference.AttemptedPath! : target;
        var normalizedOutside = PortableWorkspacePath.TryNormalize(outsidePath, out var candidatePath) ? candidatePath : outsidePath;
        var outside = layout.OutsideRoutes.SingleOrDefault(route => normalizedOutside.StartsWith(route.DirectoryPath + "/", StringComparison.Ordinal));
        if (!isPathSelection && outside is not null)
            return new RemoveTargetResolution(RemoveKind.Route, target, null);
        if (SourceLogicalPath.IsCanonicalSource(normalizedOutside) && RemovePathSafety.ProtectsOutsideRegistration(normalizedOutside, layout))
            return new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target,
                "A registered outside directory, ancestor or root entrypoint is protected.");
        if (!layout.IsImplicitContentDestination(target)
            && !string.Equals(target, WorkspaceLayout.NativeSkillsPath, StringComparison.Ordinal)
            && outside is null)
        {
            return new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target, null);
        }

        if (!PortableWorkspacePath.TryNormalize(target, out var normalized))
        {
            return new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target, null);
        }
        target = normalized;

        var resolver = new PhysicalPathResolver();
        var logical = Path.GetFullPath(Path.Combine(
            workspace.LexicalRoot,
            target.Replace('/', Path.DirectorySeparatorChar)));
        var observed = NoFollowLeafObserver.Observe(resolver, workspace, logical, cancellationToken);
        if (observed.State == NoFollowLeafState.Missing)
        {
            if (isPathSelection && outside is not null && SourceIdentity.IsRecognizedEntrypointPath(layout, target))
                return new RemoveTargetResolution(RemoveKind.Path, target, null, IsEntrypointFile: true);
            if (outside is not null && layoutRead.Settings.Document.RemovedDirectories.Contains(target, StringComparer.Ordinal))
                return new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target, null);
            if (outside is not null && SourceFormClassifier.TryClassify(layout, target, out var missingForm)
                && (missingForm == SourceDocumentForm.Markdown || SourceFormClassifier.IsEntrypoint(missingForm)))
                return RoutePath(target);
            return new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target, null);
        }
        if (observed.State == NoFollowLeafState.RelativeFileLink)
        {
            return new RemoveTargetResolution(
                isPathSelection ? RemoveKind.Path : RemoveKind.Route,
                target,
                null);
        }
        if (observed.State is not (NoFollowLeafState.OrdinaryFile or NoFollowLeafState.Directory))
        {
            return new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target,
                observed.Failure?.DirectCause ?? "The selected path could not be safely classified.");
        }

        var catalogue = outside is null
            ? await new SourceCatalogueReader().ReadAsync(new SourceCatalogueRequest(layout, layout.SourceRoots), cancellationToken).ConfigureAwait(false)
            : (await new SourceReadSessionReader(resolver).ReadIncludedAsync(layout, cancellationToken).ConfigureAwait(false)).Catalogue;
        if (catalogue.IsCancelled)
        {
            return new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target, "Source classification was cancelled.");
        }
        if (catalogue.Issues.Any(issue => issue.Stage == SourceCatalogueIssueStage.Root
            && (outside is null || issue.AttemptedCanonicalPath == outside.DirectoryPath)))
        {
            return new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target,
                $"The {layout.MainRoot} source catalogue is unavailable, so this target cannot be classified safely.");
        }

        if (observed.State == NoFollowLeafState.OrdinaryFile)
        {
            if (isPathSelection && SourceIdentity.IsRecognizedEntrypointPath(layout, target))
            {
                return new RemoveTargetResolution(RemoveKind.Path, target, null, IsEntrypointFile: true);
            }
            var source = catalogue.FindByPath(target);
            var routedMarkdown = source?.Base.Form == SourceDocumentForm.Markdown;
            return routedMarkdown || !isPathSelection
                ? RoutePath(target)
                : new RemoveTargetResolution(RemoveKind.Path, target, null);
        }

        var entrypoints = catalogue.Sources
            .Where(source => SourceFormClassifier.IsEntrypoint(source.Base.Form)
                && string.Equals(
                    SourceLogicalPath.ReadParent(source.Identity.CanonicalBasePath),
                    target,
                    StringComparison.Ordinal))
            .OrderBy(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        if (entrypoints.Length > 1)
        {
            return new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target,
                "The selected category has more than one recognized route entrypoint.");
        }
        return entrypoints.Length == 1
            ? RoutePath(entrypoints[0].Identity.CanonicalBasePath)
            : new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target, null);
    }

    private static RemoveTargetResolution RoutePath(string path)
        => new(RemoveKind.Route, path, null)
        {
            RouteReference = SourceReferenceParser.Parse(path).Kind == SourceReferenceKind.SourcePath
                ? path
                : SourceWorkspaceRelativePath.LiteralReferencePrefix + path,
        };
}
