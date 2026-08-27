using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Context.Models.Operation;

internal sealed record ContextGraphLayer
{
    public required SourceLayerKind Kind { get; init; }

    public required string CanonicalPath { get; init; }

    public required string PhysicalPath { get; init; }

    public required FileReadState ReadState { get; init; }

    public required string? Text { get; init; }

    public required MarkdownDocumentFacts? Document { get; init; }
}

internal sealed class ContextGraphSource
{
    internal ContextGraphSource(
        SourceLogicalSource? logicalSource,
        string? id,
        string canonicalPath,
        SourceDocumentForm form,
        SourceRouteState routeState,
        string? route,
        SourceAuthoredMetadataFacts metadata,
        SourceGeneratedEntriesFacts generatedEntries,
        IEnumerable<ContextGraphLayer> layers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(generatedEntries);
        ArgumentNullException.ThrowIfNull(layers);
        var values = layers.ToArray();
        if (values.Length == 0 || values.Any(value => value is null))
        {
            throw new ArgumentException("A Context graph source requires physical layer facts.", nameof(layers));
        }

        LogicalSource = logicalSource;
        Id = id;
        CanonicalPath = canonicalPath;
        Form = form;
        RouteState = routeState;
        Route = route;
        Metadata = metadata;
        GeneratedEntries = generatedEntries;
        Layers = new ReadOnlyCollection<ContextGraphLayer>(values);
    }

    internal SourceLogicalSource? LogicalSource { get; }

    internal string? Id { get; }

    internal string CanonicalPath { get; }

    internal SourceDocumentForm Form { get; }

    internal SourceRouteState RouteState { get; }

    internal string? Route { get; }

    internal SourceAuthoredMetadataFacts Metadata { get; }

    internal SourceGeneratedEntriesFacts GeneratedEntries { get; }

    internal IReadOnlyList<ContextGraphLayer> Layers { get; }

    internal bool IsLoader => Form == SourceDocumentForm.Loader;

    internal bool IsEntrypoint => SourceFormClassifier.IsEntrypoint(Form);
}

internal sealed class ContextGraph
{
    private readonly IReadOnlyDictionary<string, ContextGraphSource> _sourcesByPath;

    internal ContextGraph(
        SourceCatalogue catalogue,
        SourceRouteFacts routeFacts,
        ContextGraphSource workspaceEntry,
        IEnumerable<ContextGraphSource> sources)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(routeFacts);
        ArgumentNullException.ThrowIfNull(workspaceEntry);
        ArgumentNullException.ThrowIfNull(sources);
        var values = sources.ToArray();
        Catalogue = catalogue;
        RouteFacts = routeFacts;
        WorkspaceEntry = workspaceEntry;
        Sources = new ReadOnlyCollection<ContextGraphSource>(values);
        var byPath = new Dictionary<string, ContextGraphSource>(StringComparer.Ordinal)
        {
            [workspaceEntry.CanonicalPath] = workspaceEntry,
        };
        foreach (var source in values)
        {
            byPath.Add(source.CanonicalPath, source);
            foreach (var overwrite in source.Layers.Where(layer => layer.Kind == SourceLayerKind.Overwrite))
            {
                byPath.Add(overwrite.CanonicalPath, source);
            }
        }

        _sourcesByPath = new ReadOnlyDictionary<string, ContextGraphSource>(byPath);
    }

    internal SourceCatalogue Catalogue { get; }

    internal SourceRouteFacts RouteFacts { get; }

    internal ContextGraphSource WorkspaceEntry { get; }

    internal IReadOnlyList<ContextGraphSource> Sources { get; }

    internal ContextGraphSource? FindByPath(string path)
        => _sourcesByPath.GetValueOrDefault(path);
}
