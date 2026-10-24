using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Framework.Sources.Loading;

internal sealed class SourceLoadingClosureGraph
{
    private readonly IReadOnlyList<string> _loaderRootPaths;
    private readonly IReadOnlyDictionary<string, SourceLoadingClosureSource> _sourcesByPath;

    internal SourceLoadingClosureGraph(SourceLoadingClosureRequest request)
    {
        foreach (var source in request.Sources)
        {
            ValidateSource(request.Layout, source);
        }

        Layout = request.Layout;
        WorkspaceEntryPath = request.WorkspaceEntryPath;
        Sources = request.Sources;
        _loaderRootPaths = request.LoaderRootPaths;
        var byPath = request.Sources.ToDictionary(source => source.Path, StringComparer.Ordinal);
        foreach (var alias in request.Aliases)
        {
            if (byPath.TryGetValue(alias.Value, out var source))
            {
                byPath.TryAdd(alias.Key, source);
            }
        }

        _sourcesByPath = byPath;
    }

    internal WorkspaceLayout Layout { get; }

    internal string WorkspaceEntryPath { get; }

    internal IReadOnlyList<SourceLoadingClosureSource> Sources { get; }

    internal SourceLoadingClosureSource? FindDirectChild(
        SourceLoadingClosureSource parent,
        string destination)
    {
        var targetPath = SourceGeneratedDestinationResolver.Resolve(
            Layout, parent.Path,
            parent.IsLoader,
            destination);
        if (targetPath is null || !_sourcesByPath.TryGetValue(targetPath, out var target))
        {
            return null;
        }

        var direct = parent.IsLoader
            ? _loaderRootPaths.Contains(target.Path, StringComparer.Ordinal)
            : string.Equals(target.ParentPath, parent.Path, StringComparison.Ordinal);
        return direct ? target : null;
    }

    /// <summary>
    /// Returns the workspace-relative path an Entries destination links to outside the primary content locations, or null.
    /// </summary>
    internal string? ReadLinkedPath(
        SourceLoadingClosureSource parent,
        string destination)
        => SourceGeneratedDestinationResolver.ResolveLinked(Layout, parent.Path, parent.IsLoader, destination);

    internal SourceLoadingClosureSource? FindLinked(string linkedPath)
        => _sourcesByPath.TryGetValue(linkedPath, out var target) && (target.IsLinked || target.Path != linkedPath) ? target : null;

    private static void ValidateSource(WorkspaceLayout layout, SourceLoadingClosureSource source)
    {
        if (source.IsLinked)
        {
            var registeredCatalogue = SourceLogicalPath.IsOutsideSource(layout, source.Path)
                && SourceFormClassifier.IsEntrypoint(source.Form) && SourceFormClassifier.Matches(layout, source.Path, source.Form);
            if (source.Form != SourceDocumentForm.Markdown && !registeredCatalogue
                || source.ParentPath is null
                || SourceLogicalPath.IsPrimarySource(layout, source.Path) || layout.SourceRoots.Contains(source.Path))
            {
                throw new ArgumentException(
                    "A linked source must be ordinary Markdown or a registered catalogue outside the primary content locations with its linking catalogue as parent.",
                    nameof(source));
            }

            return;
        }

        _ = source.Form switch
        {
            SourceDocumentForm.Loader
                or SourceDocumentForm.CanonicalEntrypoint
                or SourceDocumentForm.IndexEntrypoint
                or SourceDocumentForm.UnderscoreIndexEntrypoint
                or SourceDocumentForm.ReferencesEntrypoint
                or SourceDocumentForm.UnderscoreReferencesEntrypoint
                or SourceDocumentForm.Skill
                or SourceDocumentForm.Markdown
                or SourceDocumentForm.OverwriteCompanion => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(source),
                source.Form,
                "The source document form is not defined."),
        };
        _ = source.RouteState switch
        {
            SourceRouteState.Routed
                or SourceRouteState.Unrouted
                or SourceRouteState.Ambiguous
                or SourceRouteState.Unavailable => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(source),
                source.RouteState,
                "The source route state is not defined."),
        };
        if (!SourceFormClassifier.Matches(layout, source.Path, source.Form))
        {
            throw new ArgumentException(
                "The source loading closure path does not match its document form.",
                nameof(source));
        }
    }
}
