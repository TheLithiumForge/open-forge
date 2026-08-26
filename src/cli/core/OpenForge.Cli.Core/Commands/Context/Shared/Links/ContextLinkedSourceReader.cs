using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Links;

internal sealed class ContextLinkedSourceReader
{
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceOpenForgeMetadataParser _metadataParser = new();

    internal async ValueTask<ContextGraphSource?> ReadAsync(
        ContextGraph graph,
        SourceLinkTarget target,
        IDictionary<string, ContextGraphSource> externalSources,
        CancellationToken cancellationToken)
    {
        if (target.Path is null || target.PhysicalPath is null)
        {
            return null;
        }

        var managed = graph.FindByPath(target.Path);
        if (managed is not null)
        {
            return managed;
        }

        if (target.Path.StartsWith(".agents/", StringComparison.Ordinal)
            || !target.Path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (externalSources.TryGetValue(target.Path, out var existing))
        {
            return existing;
        }

        var read = await StrictUtf8FileReader
            .ReadAsync(target.PhysicalPath, target.Path, cancellationToken)
            .ConfigureAwait(false);
        if (read.State == FileReadState.Cancelled)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        if (read.State != FileReadState.Complete || read.Value is null)
        {
            return null;
        }

        MarkdownDocumentFacts document;
        try
        {
            document = _markdownParser.Parse(read.Value);
        }
        catch (Exception)
        {
            return null;
        }

        var source = new ContextGraphSource(
            logicalSource: null,
            id: null,
            canonicalPath: target.Path,
            form: SourceDocumentForm.Markdown,
            routeState: SourceRouteState.Unrouted,
            route: null,
            metadata: _metadataParser.Parse(document),
            generatedEntries: SourceGeneratedEntriesParser.Parse(document),
            layers: [new ContextGraphLayer
            {
                Kind = SourceLayerKind.Base,
                CanonicalPath = target.Path,
                PhysicalPath = target.PhysicalPath,
                ReadState = read.State,
                Text = read.Value,
                Document = document,
            }]);
        externalSources.Add(target.Path, source);
        return source;
    }
}
