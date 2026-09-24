using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;

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
        if (!isPathSelection
            && !(selection.State == RemoveSelectionState.Route
                && selection.Target.StartsWith(".agents/", StringComparison.Ordinal)))
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

        var target = selection.Target;
        if (!target.StartsWith(".agents/", StringComparison.Ordinal))
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

        var catalogue = await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(workspace, [SourceLogicalPath.AgentsRoot]),
            cancellationToken).ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            return new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target, "Source classification was cancelled.");
        }
        if (catalogue.Issues.Any(issue => issue.Stage == SourceCatalogueIssueStage.Root))
        {
            return new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target,
                "The .agents source catalogue is unavailable, so this target cannot be classified safely.");
        }

        if (observed.State == NoFollowLeafState.OrdinaryFile)
        {
            if (isPathSelection && SourceIdentity.IsRecognizedEntrypointPath(target))
            {
                return new RemoveTargetResolution(RemoveKind.Path, target, null, IsEntrypointFile: true);
            }
            var source = catalogue.FindByPath(target);
            var routedMarkdown = source?.Base.Form == SourceDocumentForm.Markdown;
            return routedMarkdown
                ? new RemoveTargetResolution(RemoveKind.Route, target, null)
                : new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target, null);
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
            ? new RemoveTargetResolution(RemoveKind.Route, entrypoints[0].Identity.CanonicalBasePath, null)
            : new RemoveTargetResolution(isPathSelection ? RemoveKind.Path : RemoveKind.Route, target, null);
    }
}
