using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;

namespace OpenForge.Cli.Core.Framework.Sources.Loading;

internal static class SourceGeneratedEntriesParser
{
    internal static SourceGeneratedEntriesFacts Parse(MarkdownDocumentFacts document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var region = document.GeneratedRegion;
        if (region.State == MarkdownGeneratedRegionState.Absent)
        {
            return SourceGeneratedEntriesFacts.Absent;
        }

        if (region.State != MarkdownGeneratedRegionState.Complete || region.EntriesBlock?.Span is not { } span)
        {
            return SourceGeneratedEntriesFacts.Unavailable(
                region.Cause ?? "The generated Entries boundary is unavailable.");
        }

        var content = document.Source[span.Start..span.End].Replace("\r\n", "\n", StringComparison.Ordinal);
        if (content.Contains('\r'))
        {
            return SourceGeneratedEntriesFacts.Unavailable(
                "The generated Entries section uses an unsupported line ending.");
        }

        var lines = content.Split('\n', StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !MarkdownEntriesSectionReader.IsRetiredGuard(line))
            .ToArray();
        if (lines.Length == 0 || lines is [MarkdownEntriesSectionReader.EmptyEntry])
        {
            return SourceGeneratedEntriesFacts.Complete([]);
        }

        var entries = new List<SourceGeneratedEntry>();
        var searchStart = span.Start;
        foreach (var line in lines)
        {
            if (line == MarkdownEntriesSectionReader.EmptyEntry)
            {
                return SourceGeneratedEntriesFacts.Unavailable(
                    "The empty Entries sentinel must be the sole declaration.");
            }

            if (!MarkdownEntryRowParser.TryParse(line, out var row, out var cause))
            {
                return SourceGeneratedEntriesFacts.Unavailable(cause);
            }

            var lineStart = document.Source.IndexOf(line, searchStart, StringComparison.Ordinal);
            if (lineStart < 0 || lineStart >= span.End)
            {
                return SourceGeneratedEntriesFacts.Unavailable(
                    "A generated Entries declaration location could not be retained.");
            }

            entries.Add(new SourceGeneratedEntry(
                row.Description,
                row.Destination,
                row.Tags,
                new MarkdownTextSpan(lineStart, line.Length))
            {
                ApplyTo = row.ApplyTo,
            });
            searchStart = checked(lineStart + line.Length);
        }

        return SourceGeneratedEntriesFacts.Complete(entries);
    }

}
