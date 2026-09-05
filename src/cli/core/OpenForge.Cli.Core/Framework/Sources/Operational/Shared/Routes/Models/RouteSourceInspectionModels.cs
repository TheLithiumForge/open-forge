using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

internal sealed record RouteSourceLayerObservation(
    string Path,
    FileReadState State,
    string? Text);

internal sealed record RouteWorkspaceEntryObservation(
    RouteSourceLayerObservation Layer,
    OperationalViewState State);

internal sealed record RouteSourceObservation
{
    public required SourceLogicalSource Source { get; init; }

    public required IReadOnlyList<RouteSourceLayerObservation> Layers { get; init; }

    public required MarkdownDocumentFacts? Document { get; init; }

    public required SourceAuthoredMetadataFacts AuthoredMetadata { get; init; }

    public required FrameworkDocumentMetadataFacts FrameworkMetadata { get; init; }

    public required SourceGeneratedEntriesFacts GeneratedEntries { get; init; }

    public required RouteSourceStructureObservation Structure { get; init; }

    public required IReadOnlyList<RouteWorkspaceSourceIssue> WorkspaceIssues { get; init; }

    internal bool IsReadable => Layers.All(layer => layer.State == FileReadState.Complete);
}

internal sealed record RouteSourceInspection
{
    public required SourceCatalogue Catalogue { get; init; }

    public required SourceRouteFacts Routes { get; init; }

    public required RouteSourceLayerObservation WorkspaceEntry { get; init; }

    public required IReadOnlyList<RouteSourceObservation> Sources { get; init; }

    public required OperationalViewState State { get; init; }
}
