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
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(document);
        if (document.GeneratedRegion.EntriesBlock is not { } block) return [];
        var records = ImmutableArray.CreateBuilder<RenameRecordedIdentity>();
        foreach (var line in document.Source.AsSpan(block.Span.Start, block.Span.Length).EnumerateLines())
        {
            if (!MarkdownEntryRowParser.TryParse(line.ToString(), out var row, out _) || row.Tags.Count == 0) continue;
            var path = SourceGeneratedDestinationResolver.Resolve(parentPath, parentPath == SourceLogicalPath.LoaderPath, row.Destination);
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
