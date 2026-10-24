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
/// Every row retains its authored metadata. The first reaching row supplies the legacy source projection.
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
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var traversed = new HashSet<string>(StringComparer.Ordinal);
        var known = catalogue.Sources.ToDictionary(source => source.Base.CanonicalPath, StringComparer.Ordinal);
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
            source.Base.Form == SourceDocumentForm.Loader || SourceFormClassifier.IsEntrypoint(source.Base.Form) || source.IsIncludedCatalogue));
        while (pending.TryDequeue(out var parent))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!traversed.Add(parent.Base.CanonicalPath)) continue;
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
                    catalogue.Layout, parent.Base.CanonicalPath, parent.Base.Form == SourceDocumentForm.Loader, entry.Destination);
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
                if (physical.TryGetValue(physicalPath, out var firstPath) && firstPath != path)
                {
                    rows[^1] = row with { AliasOf = firstPath };
                    continue;
                }

                physical.TryAdd(physicalPath, path);

                var existing = known.GetValueOrDefault(path);
                var layer = existing?.Base ?? new SourceLayer(canonicalPath: path, physicalPath: resolution.GetContainedPhysicalPath(),
                    form: SourceDocumentForm.Markdown, kind: SourceLayerKind.Base);
                var linkedRead = await reader.ReadAsync(layer, cancellationToken).ConfigureAwait(false);
                if (linkedRead.Read is not { State: FileReadState.Complete, Value: { } linkedText })
                {
                    continue;
                }

                var isCatalogue = SourceGeneratedDestinationResolver.IsMarkdown(path)
                    && SourceGeneratedEntriesParser.Parse(reader.ParseDocument(linkedText)).State == SourceGeneratedEntriesState.Complete;
                var source = new SourceLogicalSource(existing?.Identity ?? new SourceLogicalIdentity(automaticId: SourceWorkspaceRelativePath.LiteralReferencePrefix + path, canonicalBasePath: path), layer, existing?.Overwrite)
                {
                    IncludedParentPath = existing?.IncludedParentPath ?? parent.Base.CanonicalPath,
                    IncludedEntry = existing?.IncludedEntry ?? entry,
                    IsIncludedCatalogue = isCatalogue,
                };
                included.Add(source);
                known[path] = source;
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

        var sources = catalogue.Sources.ToDictionary(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal);
        var physical = new Dictionary<string, string>(PhysicalIdentityTracker.PathComparer);
        foreach (var source in catalogue.Sources)
            physical.TryAdd(source.Base.PhysicalPath, source.Base.CanonicalPath);
        foreach (var source in included)
        {
            if (physical.TryGetValue(source.Base.PhysicalPath, out var firstPath) && firstPath != source.Base.CanonicalPath)
            {
                aliases.TryAdd(source.Base.CanonicalPath, firstPath);
                continue;
            }
            physical.TryAdd(source.Base.PhysicalPath, source.Base.CanonicalPath);
            if (!sources.TryGetValue(source.Identity.CanonicalBasePath, out var existing) || existing.IncludedEntry is null)
                sources[source.Identity.CanonicalBasePath] = source;
        }
        var retainedRows = catalogue.IncludedRows.Concat(rows ?? []).ToArray();
        foreach (var row in retainedRows)
        {
            var path = row.AliasOf ?? aliases.GetValueOrDefault(row.Path) ?? row.Path;
            if (SourceLogicalPath.IsPrimarySource(catalogue.Layout, path) || !sources.TryGetValue(path, out var source) || source.IncludedEntry is not null) continue;
            sources[path] = new SourceLogicalSource(source.Identity, source.Base, source.Overwrite)
            {
                IncludedParentPath = row.ParentPath,
                IncludedEntry = row.Entry,
                IsIncludedCatalogue = source.IsIncludedCatalogue,
            };
        }
        return new SourceCatalogue(catalogue.Layout, catalogue.Candidates, sources.Values, catalogue.Issues, catalogue.IsCancelled, aliases, retainedRows);
    }
}
