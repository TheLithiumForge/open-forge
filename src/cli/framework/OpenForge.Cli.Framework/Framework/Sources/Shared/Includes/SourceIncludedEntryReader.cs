using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Framework.Sources.Shared.Includes;

/// <summary>
/// Reads workspace files reached through catalogue Entries, without scanning their folders.
/// The first row reaching a physical file owns its parent and row metadata.
/// </summary>
internal sealed class SourceIncludedEntryReader
{
    internal async ValueTask<IReadOnlyList<SourceLogicalSource>> ReadAsync(
        SourceCatalogue catalogue,
        SourceDocumentReader reader,
        CancellationToken cancellationToken)
        => (await ReadEntriesAsync(catalogue, reader, cancellationToken).ConfigureAwait(false)).Sources;

    internal async ValueTask<SourceIncludedEntryRead> ReadEntriesAsync(
        SourceCatalogue catalogue,
        SourceDocumentReader reader,
        CancellationToken cancellationToken)
    {
        var included = new List<SourceLogicalSource>();
        var rows = new List<SourceIncludedEntryRow>();
        var seen = new HashSet<string>(catalogue.Sources.Select(source => source.Base.CanonicalPath), StringComparer.Ordinal);
        var physical = new Dictionary<string, string>(PhysicalIdentityTracker.PathComparer);
        foreach (var source in catalogue.Sources)
        {
            physical.TryAdd(source.Base.PhysicalPath, source.Base.CanonicalPath);
            if (source.Overwrite is { } overwrite)
            {
                physical.TryAdd(overwrite.PhysicalPath, source.Base.CanonicalPath);
            }
        }
        var pending = new Queue<SourceLogicalSource>(catalogue.Sources.Where(source =>
            source.Base.Form == SourceDocumentForm.Loader || SourceFormClassifier.IsEntrypoint(source.Base.Form)));
        while (pending.TryDequeue(out var parent))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = await reader.ReadAsync(parent.Base, cancellationToken).ConfigureAwait(false);
            if (read.Read is not { State: FileReadState.Complete, Value: { } text })
            {
                continue;
            }

            var entries = SourceGeneratedEntriesParser.Parse(reader.ParseDocument(text));
            if (entries.State != SourceGeneratedEntriesState.Complete)
            {
                continue;
            }

            foreach (var entry in entries.Entries)
            {
                var path = SourceGeneratedDestinationResolver.ResolveLinked(
                    parent.Base.CanonicalPath, parent.Base.Form == SourceDocumentForm.Loader, entry.Destination);
                if (path is null)
                {
                    continue;
                }

                var resolution = reader.PhysicalPaths.ResolveCandidate(
                    reader.Workspace.LexicalRoot, reader.Workspace.PhysicalRoot,
                    Path.Combine(reader.Workspace.LexicalRoot, path.Replace('/', Path.DirectorySeparatorChar)));
                var row = new SourceIncludedEntryRow(Path: path, ParentPath: parent.Base.CanonicalPath, Entry: entry, PhysicalState: resolution.State);
                rows.Add(row);
                if (!seen.Add(path) || resolution.State != PhysicalPathState.Contained)
                {
                    continue;
                }

                var physicalPath = resolution.GetContainedPhysicalPath();
                if (physical.TryGetValue(physicalPath, out var firstPath))
                {
                    rows[^1] = row with { AliasOf = firstPath };
                    continue;
                }

                physical.Add(physicalPath, path);

                var layer = new SourceLayer(canonicalPath: path, physicalPath: resolution.GetContainedPhysicalPath(),
                    form: SourceDocumentForm.Markdown, kind: SourceLayerKind.Base);
                var linkedRead = await reader.ReadAsync(layer, cancellationToken).ConfigureAwait(false);
                if (linkedRead.Read is not { State: FileReadState.Complete, Value: { } linkedText })
                {
                    continue;
                }

                var isCatalogue = SourceGeneratedDestinationResolver.IsMarkdown(path)
                    && SourceGeneratedEntriesParser.Parse(reader.ParseDocument(linkedText)).State == SourceGeneratedEntriesState.Complete;
                var source = new SourceLogicalSource(new SourceLogicalIdentity(automaticId: $"./{path}", canonicalBasePath: path), layer)
                {
                    IncludedParentPath = parent.Base.CanonicalPath,
                    IncludedEntry = entry,
                    IsIncludedCatalogue = isCatalogue,
                };
                included.Add(source);
                if (isCatalogue)
                {
                    pending.Enqueue(source);
                }
            }
        }

        return new SourceIncludedEntryRead(included, rows);
    }

    internal static SourceCatalogue Extend(SourceCatalogue catalogue, IEnumerable<SourceLogicalSource> included, IReadOnlyList<SourceIncludedEntryRow>? rows = null)
    {
        var aliases = catalogue.Aliases.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        foreach (var row in rows ?? [])
        {
            if (row.AliasOf is { } firstPath)
            {
                aliases.TryAdd(row.Path, firstPath);
            }
        }

        return new SourceCatalogue(catalogue.Workspace, catalogue.Candidates, catalogue.Sources.Concat(included), catalogue.Issues, catalogue.IsCancelled, aliases);
    }
}
