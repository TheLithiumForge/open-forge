using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
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

            if (!TryParseEntry(
                line,
                out var description,
                out var destination,
                out var tags,
                out var cause))
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
                description,
                destination,
                tags,
                new MarkdownTextSpan(lineStart, line.Length)));
            searchStart = checked(lineStart + line.Length);
        }

        return SourceGeneratedEntriesFacts.Complete(entries);
    }

    private static bool TryParseEntry(
        string line,
        [NotNullWhen(true)] out string? description,
        [NotNullWhen(true)] out string? destination,
        out IReadOnlyList<string> tags,
        [NotNullWhen(false)] out string? cause)
    {
        description = null;
        destination = null;
        tags = [];
        cause = null;
        if (!line.StartsWith("- [", StringComparison.Ordinal))
        {
            cause = "A generated Entries declaration must start with a single hyphen list marker and label.";
            return false;
        }

        var destinationStart = line.IndexOf("](", 3, StringComparison.Ordinal);
        if (destinationStart < 0 || destinationStart == 3 || !IsCanonicalLinkLabel(line.AsSpan(3, destinationStart - 3)))
        {
            cause = "A generated Entries declaration must contain a non-empty inline link label.";
            return false;
        }

        description = line[3..destinationStart];

        var destinationEnd = line.IndexOf(')', destinationStart + 2);
        if (destinationEnd < 0 || destinationEnd == destinationStart + 2)
        {
            cause = "A generated Entries declaration must contain a non-empty destination.";
            return false;
        }

        destination = line[(destinationStart + 2)..destinationEnd];
        var suffix = line[(destinationEnd + 1)..];
        if (suffix.Length == 0)
        {
            return true;
        }

        const string separator = " - ";
        if (!suffix.StartsWith(separator, StringComparison.Ordinal))
        {
            cause = "A generated Entries declaration must use the canonical optional tag separator.";
            return false;
        }

        var values = suffix[separator.Length..].Split(' ', StringSplitOptions.None);
        if (values.Length == 0 || values.Any(value => value.Length < 2
                || value[0] != '#'
                || !SourceOpenForgeMetadataParser.IsValidTag(value[1..])))
        {
            cause = "A generated Entries declaration must contain canonical bare tags.";
            return false;
        }

        tags = values.Select(value => value[1..]).ToArray();
        return true;
    }

    private static bool IsCanonicalLinkLabel(ReadOnlySpan<char> value)
    {
        var hasNonWhitespace = false;
        foreach (var character in value)
        {
            if (char.IsControl(character) || character is '[' or ']')
            {
                return false;
            }

            hasNonWhitespace |= !char.IsWhiteSpace(character);
        }

        return hasNonWhitespace;
    }
}
