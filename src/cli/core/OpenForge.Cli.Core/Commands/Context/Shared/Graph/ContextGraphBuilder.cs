using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
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
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Graph;

internal sealed class ContextGraphBuilder
{
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();

    internal async ValueTask<ContextGraph> BuildAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        cancellationToken.ThrowIfCancellationRequested();
        var catalogue = await new SourceCatalogueReader()
            .ReadAsync(
                new SourceCatalogueRequest(workspace, [SourceLogicalPath.AgentsRoot]),
                cancellationToken)
            .ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var reader = new SourceDocumentReader(workspace);
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

        return new ContextGraph(
            catalogue: catalogue,
            routeFacts: routeFacts,
            workspaceEntry: await ReadWorkspaceEntryAsync(workspace, cancellationToken).ConfigureAwait(false),
            sources: sources);
    }

    private async ValueTask<ContextGraphSource> ReadWorkspaceEntryAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        const string logicalPath = "AGENTS.md";
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
            read = await StrictUtf8FileReader
                .ReadAsync(physicalPath, logicalPath, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            physicalPath = Path.GetFullPath(lexicalPath);
            read = FileReadResult<string>.Missing(logicalPath);
        }

        var document = read.State == FileReadState.Complete && read.Value is not null
            ? _markdownParser.Parse(read.Value)
            : null;
        return new ContextGraphSource(
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
            ? _markdownParser.Parse(text)
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
