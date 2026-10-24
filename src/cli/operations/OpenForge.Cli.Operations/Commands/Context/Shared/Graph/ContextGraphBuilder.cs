using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Sources.Shared.Includes;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Graph;

internal sealed class ContextGraphBuilder
{
    private readonly SourceAuthoredMetadataParser _metadataParser = new();

    internal async ValueTask<ContextGraph> BuildAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        cancellationToken.ThrowIfCancellationRequested();
        var paths = new PhysicalPathResolver();
        var observation = await new WorkspaceLayoutReader(paths).ReadAsync(workspace, cancellationToken).ConfigureAwait(false);
        var layout = observation.Layout ?? throw new InvalidOperationException("The workspace layout is unavailable.");
        var session = await new SourceReadSessionReader(paths)
            .ReadAsync(layout, cancellationToken).ConfigureAwait(false);
        var catalogue = session.Catalogue;
        cancellationToken.ThrowIfCancellationRequested();
        var reader = session.DocumentReader;
        var selection = catalogue.SelectAll();
        var routeFacts = await new SourceRouteFactsResolver()
            .ResolveAsync(
                new SourceRouteFactsRequest(catalogue, selection),
                reader,
                cancellationToken)
            .ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        var sources = new List<ContextGraphSource>();
        foreach (var source in catalogue.Sources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var layers = new List<ContextGraphLayer>
            {
                await ReadLayerAsync(reader, source.Base, cancellationToken).ConfigureAwait(false),
            };
            if (source.Overwrite is { } overwrite)
            {
                layers.Add(await ReadLayerAsync(reader, overwrite, cancellationToken).ConfigureAwait(false));
            }

            var baseDocument = layers[0].Document;
            var metadata = baseDocument is null
                ? SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Malformed)
                : _metadataParser.Parse(baseDocument, source.Base.Form);
            var generated = baseDocument is null
                ? SourceGeneratedEntriesFacts.Unavailable("The source document is unavailable.")
                : SourceGeneratedEntriesParser.Parse(baseDocument);
            var route = routeFacts.RouteFacts.SingleOrDefault(fact => ReferenceEquals(fact.Identity, source.Identity));
            sources.Add(new ContextGraphSource(
                layout: layout,
                logicalSource: source,
                id: source.Identity.AutomaticId,
                canonicalPath: source.Identity.CanonicalBasePath,
                form: source.Base.Form,
                routeState: route?.State ?? SourceRouteState.Unrouted,
                route: route?.Route,
                metadata: metadata,
                generatedEntries: generated,
                layers: layers));
        }

        var included = await new SourceIncludedEntryReader().ReadEntriesAsync(catalogue, reader, cancellationToken).ConfigureAwait(false);
        foreach (var source in included.Sources)
        {
            var layer = await ReadLayerAsync(reader, source.Base, cancellationToken).ConfigureAwait(false);
            var document = layer.Document ?? throw new InvalidOperationException("An included source requires a readable document.");
            var entry = source.IncludedEntry ?? throw new InvalidOperationException("An included source requires its catalogue row.");
            sources.Add(new ContextGraphSource(layout: layout, logicalSource: null, id: null, canonicalPath: source.Base.CanonicalPath,
                form: SourceDocumentForm.Markdown, routeState: SourceRouteState.Unrouted, route: null,
                metadata: _metadataParser.Parse(document, SourceDocumentForm.Markdown),
                generatedEntries: SourceGeneratedDestinationResolver.IsMarkdown(source.Base.CanonicalPath)
                    ? SourceGeneratedEntriesParser.Parse(document) : SourceGeneratedEntriesFacts.Absent, layers: [layer])
            {
                LinkedParentPath = source.IncludedParentPath,
                LinkedApplyTo = ApplyToMetadataFacts.FromEntryLine(entry.ApplyTo),
            });
        }

        catalogue = SourceIncludedEntryReader.Extend(catalogue, included.Sources, included.Rows);
        return new ContextGraph(
            layoutRead: observation,
            catalogue: catalogue,
            routeFacts: routeFacts,
            workspaceEntry: await ReadWorkspaceEntryAsync(layout, reader, cancellationToken).ConfigureAwait(false),
            sources: sources);
    }

    private async ValueTask<ContextGraphSource> ReadWorkspaceEntryAsync(
        WorkspaceLayout layout,
        SourceDocumentReader reader,
        CancellationToken cancellationToken)
    {
        var workspace = reader.Workspace;
        const string logicalPath = SourceLogicalPath.WorkspaceEntryPath;
        var lexicalPath = Path.Combine(workspace.LexicalRoot, logicalPath);
        var resolution = new PhysicalPathResolver().ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            lexicalPath);
        FileReadResult<string> read;
        string physicalPath;
        if (resolution.State == PhysicalPathState.Contained)
        {
            physicalPath = resolution.GetContainedPhysicalPath();
            read = await reader.Acquisition
                .ReadAsync(physicalPath, logicalPath, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            physicalPath = Path.GetFullPath(lexicalPath);
            read = FileReadResult<string>.Missing(logicalPath);
        }

        var document = read.State == FileReadState.Complete && read.Value is not null
            ? reader.ParseDocument(read.Value)
            : null;
        return new ContextGraphSource(
            layout: layout,
            logicalSource: null,
            id: null,
            canonicalPath: logicalPath,
            form: SourceDocumentForm.Markdown,
            routeState: SourceRouteState.Unrouted,
            route: null,
            metadata: SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing),
            generatedEntries: SourceGeneratedEntriesFacts.Absent,
            layers: [new ContextGraphLayer
            {
                Kind = SourceLayerKind.Base,
                CanonicalPath = logicalPath,
                PhysicalPath = physicalPath,
                ReadState = read.State,
                Text = read.Value,
                Document = document,
            }]);
    }

    private async ValueTask<ContextGraphLayer> ReadLayerAsync(
        SourceDocumentReader reader,
        SourceLayer layer,
        CancellationToken cancellationToken)
    {
        var result = await reader.ReadAsync(layer, cancellationToken).ConfigureAwait(false);
        var state = ReadState(result);
        var text = result.Read?.Value;
        var document = state == FileReadState.Complete && text is not null
            ? reader.ParseDocument(text)
            : null;
        return new ContextGraphLayer
        {
            Kind = layer.Kind,
            CanonicalPath = layer.CanonicalPath,
            PhysicalPath = layer.PhysicalPath,
            ReadState = state,
            Text = text,
            Document = document,
        };
    }

    private static FileReadState ReadState(SourceDocumentReadResult result)
        => result.Read?.State ?? result.Verification.State switch
        {
            SourceLayerVerificationState.Missing => FileReadState.Missing,
            SourceLayerVerificationState.Cancelled => FileReadState.Cancelled,
            _ => FileReadState.InputOutputFailure,
        };
}
