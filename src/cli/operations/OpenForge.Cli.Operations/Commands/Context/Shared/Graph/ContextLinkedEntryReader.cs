using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Graph;

/// <summary>
/// Reads ordinary Markdown files outside `.agents/` that Entries lines link to, such as
/// `../docs/architecture.md`, and follows the Entries of linked catalogues. Each file is read once,
/// stays read-only, and keeps the first catalogue that linked it as its parent.
/// </summary>
internal sealed class ContextLinkedEntryReader
{
    private readonly SourceAuthoredMetadataParser _metadataParser = new();
    private readonly PhysicalPathResolver _physicalPathResolver = new();

    internal async ValueTask<IReadOnlyList<ContextGraphSource>> ReadAsync(
        SourceDocumentReader reader,
        IReadOnlyList<ContextGraphSource> sources,
        CancellationToken cancellationToken)
    {
        var linked = new List<ContextGraphSource>();
        var seen = new HashSet<string>(sources.Select(source => source.CanonicalPath), StringComparer.Ordinal);
        var pending = new Queue<ContextGraphSource>(sources.Where(source => source.IsLoader || source.IsEntrypoint));
        while (pending.TryDequeue(out var parent))
        {
            if (parent.GeneratedEntries.State != SourceGeneratedEntriesState.Complete)
            {
                continue;
            }

            foreach (var entry in parent.GeneratedEntries.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var linkedPath = SourceGeneratedDestinationResolver.ResolveLinked(
                    parent.CanonicalPath,
                    parent.IsLoader,
                    entry.Destination);
                if (linkedPath is null || !seen.Add(linkedPath))
                {
                    continue;
                }

                var source = await ReadLinkedAsync(reader, parent, entry, linkedPath, cancellationToken).ConfigureAwait(false);
                if (source is null)
                {
                    continue;
                }

                linked.Add(source);
                pending.Enqueue(source);
            }
        }

        return linked;
    }

    private async ValueTask<ContextGraphSource?> ReadLinkedAsync(
        SourceDocumentReader reader,
        ContextGraphSource parent,
        SourceGeneratedEntry entry,
        string linkedPath,
        CancellationToken cancellationToken)
    {
        var workspace = reader.Workspace;
        var resolution = _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            Path.Combine(workspace.LexicalRoot, linkedPath.Replace('/', Path.DirectorySeparatorChar)));
        if (resolution.State != PhysicalPathState.Contained)
        {
            return null;
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        var read = await StrictUtf8FileReader.ReadAsync(physicalPath, linkedPath, cancellationToken).ConfigureAwait(false);
        if (read.State == FileReadState.Cancelled)
        {
            throw new OperationCanceledException(cancellationToken);
        }

        if (read.State != FileReadState.Complete || read.Value is null)
        {
            return null;
        }

        var document = reader.ParseDocument(read.Value);
        return new ContextGraphSource(
            logicalSource: null,
            id: null,
            canonicalPath: linkedPath,
            form: SourceDocumentForm.Markdown,
            routeState: SourceRouteState.Unrouted,
            route: null,
            metadata: _metadataParser.Parse(document, SourceDocumentForm.Markdown),
            generatedEntries: SourceGeneratedEntriesParser.Parse(
                SourceGeneratedDestinationResolver.IsMarkdown(linkedPath) ? document : reader.ParseDocument(string.Empty)),
            layers: [new ContextGraphLayer
            {
                Kind = SourceLayerKind.Base,
                CanonicalPath = linkedPath,
                PhysicalPath = physicalPath,
                ReadState = read.State,
                Text = read.Value,
                Document = document,
            }])
        {
            LinkedParentPath = parent.CanonicalPath,
            LinkedApplyTo = ApplyToMetadataFacts.FromEntryLine(entry.ApplyTo),
        };
    }
}
