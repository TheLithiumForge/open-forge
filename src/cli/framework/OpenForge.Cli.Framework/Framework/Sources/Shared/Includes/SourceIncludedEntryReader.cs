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
        foreach (var source in catalogue.Sources.OrderBy(source => SourceLogicalPath.IsOutsideSource(catalogue.Layout, source.Base.CanonicalPath)))
        {
            if (!SourceLogicalPath.IsOutsideSource(catalogue.Layout, source.Base.CanonicalPath) || source.IncludedEntry is not null)
                physical.TryAdd(source.Base.PhysicalPath, source.Base.CanonicalPath);
            if (source.Overwrite is { } overwrite)
            {
                physical.TryAdd(overwrite.PhysicalPath, source.Base.CanonicalPath);
            }
        }
        var catalogues = catalogue.Sources.Where(source =>
            source.Base.Form == SourceDocumentForm.Loader || SourceFormClassifier.IsEntrypoint(source.Base.Form) || source.IsIncludedCatalogue).ToArray();
        var pending = new Queue<SourceLogicalSource>(catalogues.Where(source => !SourceLogicalPath.IsOutsideSource(catalogue.Layout, source.Base.CanonicalPath)));
        var disconnected = new Queue<SourceLogicalSource>(catalogues.Where(source => SourceLogicalPath.IsOutsideSource(catalogue.Layout, source.Base.CanonicalPath))
            .OrderBy(source => source.Base.CanonicalPath, StringComparer.Ordinal));
        while (pending.Count > 0 || disconnected.Count > 0)
        {
            var parent = pending.Count > 0 ? pending.Dequeue() : disconnected.Dequeue();
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
                if (resolution.State != PhysicalPathState.Contained)
                {
                    continue;
                }

                var physicalPath = resolution.GetContainedPhysicalPath();
                if (physical.TryGetValue(physicalPath, out var firstPath) && firstPath != path)
                {
                    rows[^1] = row with { AliasOf = firstPath };
                    if (!SourceLogicalPath.IsOutsideSource(catalogue.Layout, firstPath)) continue;
                    path = firstPath;
                }

                physical.TryAdd(physicalPath, path);
                if (!seen.Add(path)) continue;

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

        var includedSources = included.ToArray();
        var sources = catalogue.Sources.ToDictionary(source => source.Identity.CanonicalBasePath, StringComparer.Ordinal);
        foreach (var source in includedSources)
        {
            if (!sources.TryGetValue(source.Identity.CanonicalBasePath, out var existing) || existing.IncludedEntry is null)
                sources[source.Identity.CanonicalBasePath] = source;
        }
        var retainedRows = catalogue.IncludedRows.Concat(rows ?? []).ToArray();
        var projectedPaths = catalogue.Sources.Where(source => source.IncludedEntry is not null)
            .Select(source => source.Base.CanonicalPath).ToHashSet(StringComparer.Ordinal);
        foreach (var row in retainedRows)
        {
            var path = row.AliasOf ?? aliases.GetValueOrDefault(row.Path) ?? row.Path;
            if (SourceLogicalPath.IsPrimarySource(catalogue.Layout, path) || !sources.TryGetValue(path, out var source) || !projectedPaths.Add(path)) continue;
            sources[path] = new SourceLogicalSource(source.Identity, source.Base, source.Overwrite)
            {
                IncludedParentPath = row.ParentPath,
                IncludedEntry = row.Entry,
                IsIncludedCatalogue = source.IsIncludedCatalogue,
            };
        }
        var preferredPaths = catalogue.Sources
            .Where(source => SourceLogicalPath.IsPrimarySource(catalogue.Layout, source.Base.CanonicalPath))
            .Select(source => source.Base.CanonicalPath)
            .Concat(catalogue.Sources.Where(source => source.IncludedEntry is not null).Select(source => source.Base.CanonicalPath))
            .Concat(retainedRows.Where(row => row.PhysicalState == PhysicalPathState.Contained)
                .Select(row => row.AliasOf ?? aliases.GetValueOrDefault(row.Path) ?? row.Path))
            .Concat(includedSources.Select(source => source.Base.CanonicalPath))
            .Concat(sources.Keys)
            .Distinct(StringComparer.Ordinal);
        var retainedSources = new List<SourceLogicalSource>();
        var physical = new Dictionary<string, string>(PhysicalIdentityTracker.PathComparer);
        foreach (var path in preferredPaths)
        {
            if (!sources.TryGetValue(path, out var source)) continue;
            if (SourceLogicalPath.IsPrimarySource(catalogue.Layout, path))
            {
                physical.TryAdd(source.Base.PhysicalPath, path);
                retainedSources.Add(source);
                continue;
            }
            if (physical.TryGetValue(source.Base.PhysicalPath, out var firstPath))
            {
                aliases[path] = firstPath;
                continue;
            }
            physical.Add(source.Base.PhysicalPath, path);
            retainedSources.Add(source);
        }
        foreach (var alias in aliases.Keys.ToArray())
        {
            if (sources.TryGetValue(aliases[alias], out var source) && physical.TryGetValue(source.Base.PhysicalPath, out var firstPath))
                aliases[alias] = firstPath;
            if (alias == aliases[alias]) aliases.Remove(alias);
        }
        return new SourceCatalogue(catalogue.Layout, catalogue.Candidates, retainedSources, catalogue.Issues, catalogue.IsCancelled, aliases, retainedRows);
    }
}
