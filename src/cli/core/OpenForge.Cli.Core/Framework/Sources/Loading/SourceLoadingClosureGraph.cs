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
            ValidateSource(source);
        }

        WorkspaceEntryPath = request.WorkspaceEntryPath;
        Sources = request.Sources;
        _loaderRootPaths = request.LoaderRootPaths;
        _sourcesByPath = request.Sources.ToDictionary(source => source.Path, StringComparer.Ordinal);
    }

    internal string WorkspaceEntryPath { get; }

    internal IReadOnlyList<SourceLoadingClosureSource> Sources { get; }

    internal SourceLoadingClosureSource? FindDirectChild(
        SourceLoadingClosureSource parent,
        string destination)
    {
        var targetPath = SourceGeneratedDestinationResolver.Resolve(
            parent.Path,
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

    internal IReadOnlyList<SourceLoadingClosureSource>? ReadRouteChain(
        SourceLoadingClosureSource source)
    {
        var chain = new List<SourceLoadingClosureSource>();
        var current = source;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        while (seen.Add(current.Path))
        {
            chain.Add(current);
            if (current.ParentPath is null)
            {
                chain.Reverse();
                return _loaderRootPaths.Contains(current.Path, StringComparer.Ordinal)
                    ? chain
                    : null;
            }

            if (!_sourcesByPath.TryGetValue(current.ParentPath, out current))
            {
                return null;
            }
        }

        return null;
    }

    private static void ValidateSource(SourceLoadingClosureSource source)
    {
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
        if (!SourceFormClassifier.Matches(source.Path, source.Form))
        {
            throw new ArgumentException(
                "The source loading closure path does not match its document form.",
                nameof(source));
        }
    }
}
