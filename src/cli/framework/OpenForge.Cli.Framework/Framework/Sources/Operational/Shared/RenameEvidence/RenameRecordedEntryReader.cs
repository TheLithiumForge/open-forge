using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

internal static class RenameRecordedEntryReader
{
    internal static ImmutableArray<RenameRecordedIdentity> Read(string parentPath, FileStateSnapshot snapshot, MarkdownDocumentFacts document)
        => Read(SourceContentRoots.Default, parentPath, snapshot, document);

    internal static ImmutableArray<RenameRecordedIdentity> Read(WorkspaceLayout layout, string parentPath, FileStateSnapshot snapshot, MarkdownDocumentFacts document)
        => Read(new SourceContentRoots(layout), parentPath, snapshot, document);

    private static ImmutableArray<RenameRecordedIdentity> Read(SourceContentRoots roots, string parentPath, FileStateSnapshot snapshot, MarkdownDocumentFacts document)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(document);
        if (document.GeneratedRegion.EntriesBlock is not { } block) return [];
        var records = ImmutableArray.CreateBuilder<RenameRecordedIdentity>();
        foreach (var line in document.Source.AsSpan(block.Span.Start, block.Span.Length).EnumerateLines())
        {
            if (!MarkdownEntryRowParser.TryParse(line.ToString(), out var row, out _) || row.Tags.Count == 0) continue;
            var path = SourceGeneratedDestinationResolver.Resolve(roots, parentPath, parentPath == roots.LoaderPath, row.Destination);
            if (path is null) continue;
            records.Add(new RenameRecordedIdentity
            {
                Path = path,
                Metadata = RenameMetadataIdentity.Create(row.Description, row.Tags, row.ApplyTo.Select(pattern => pattern.Text)),
                RecordExpectation = snapshot.Expectation,
            });
        }

        return records.ToImmutable();
    }
}
