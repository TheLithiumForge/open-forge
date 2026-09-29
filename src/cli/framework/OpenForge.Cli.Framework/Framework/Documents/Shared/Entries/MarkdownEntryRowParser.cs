using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Shared.Entries;

internal static class MarkdownEntryRowParser
{
    private const string TagSeparator = " - ";
    private const string ApplyToSeparator = " - applies to ";
    private const string ApplyToLabel = "applies to ";

    internal static bool TryParse(
        string line,
        [NotNullWhen(true)] out MarkdownEntryRow? row,
        [NotNullWhen(false)] out string? cause)
    {
        row = null;
        cause = null;
        if (!line.StartsWith("- [", StringComparison.Ordinal))
        {
            cause = "A generated Entries declaration must start with a single hyphen list marker and label.";
            return false;
        }

        var destinationStart = line.IndexOf("](", 3, StringComparison.Ordinal);
        if (destinationStart < 0
            || destinationStart == 3
            || !IsCanonicalLinkLabel(line.AsSpan(3, destinationStart - 3)))
        {
            cause = "A generated Entries declaration must contain a non-empty inline link label.";
            return false;
        }

        var destinationEnd = line.IndexOf(')', destinationStart + 2);
        if (destinationEnd < 0 || destinationEnd == destinationStart + 2)
        {
            cause = "A generated Entries declaration must contain a non-empty destination.";
            return false;
        }

        var suffix = line[(destinationEnd + 1)..];
        IReadOnlyList<string> tags = [];
        IReadOnlyList<ApplyToPattern> applyTo = [];
        if (suffix.Length > 0)
        {
            if (!suffix.StartsWith(TagSeparator, StringComparison.Ordinal))
            {
                cause = "A generated Entries declaration must use the canonical optional tag separator.";
                return false;
            }

            var content = suffix[TagSeparator.Length..];
            if (content.StartsWith(ApplyToLabel, StringComparison.Ordinal))
            {
                if (!TryParsePatterns(content[ApplyToLabel.Length..], out var parsedApplyTo, out cause))
                {
                    return false;
                }

                applyTo = parsedApplyTo;
            }
            else
            {
                var applyToStart = FindApplyToSeparator(content);
                var tagText = applyToStart < 0 ? content : content[..applyToStart];
                if (tagText.Length > 0)
                {
                    var values = tagText.Split(' ', StringSplitOptions.None);
                    if (values.Any(value => value.Length < 2
                            || value[0] != '#'
                            || !FrameworkDocumentMetadataTagGrammar.IsValid(value[1..])))
                    {
                        cause = "A generated Entries declaration must contain canonical bare tags.";
                        return false;
                    }

                    tags = values.Select(value => value[1..]).ToArray();
                }

                if (applyToStart >= 0)
                {
                    if (!TryParsePatterns(
                        content[(applyToStart + ApplyToSeparator.Length)..],
                        out var parsedApplyTo,
                        out cause))
                    {
                        return false;
                    }

                    applyTo = parsedApplyTo;
                }
                else if (tags.Count == 0)
                {
                    cause = "A generated Entries declaration cannot contain an empty suffix.";
                    return false;
                }
            }
        }

        row = new MarkdownEntryRow(
            line[3..destinationStart],
            line[(destinationStart + 2)..destinationEnd],
            tags,
            applyTo);
        return true;
    }

    private static bool TryParsePatterns(
        string source,
        [NotNullWhen(true)] out IReadOnlyList<ApplyToPattern>? patterns,
        [NotNullWhen(false)] out string? cause)
    {
        patterns = null;
        cause = null;
        var values = new List<ApplyToPattern>();
        var position = 0;
        while (position < source.Length)
        {
            if (source[position] != '`')
            {
                cause = "An applies to suffix must contain comma-separated code spans.";
                return false;
            }

            var fenceLength = CountRun(source, position, '`');
            var contentStart = position + fenceLength;
            var contentEnd = FindFence(source, contentStart, fenceLength);
            if (contentEnd < 0)
            {
                cause = "An applies to code span is not closed.";
                return false;
            }

            var text = NormalizeCodeSpanContent(source[contentStart..contentEnd]);
            var parsed = ApplyToPatternMatcher.Parse(text);
            if (parsed.Pattern is not { } pattern)
            {
                cause = "An applies to code span contains an invalid path pattern.";
                return false;
            }

            values.Add(pattern);
            position = contentEnd + fenceLength;
            if (position == source.Length)
            {
                break;
            }

            if (!source.AsSpan(position).StartsWith(", ", StringComparison.Ordinal))
            {
                cause = "ApplyTo patterns must use the canonical comma and space separator.";
                return false;
            }

            position += 2;
            if (position == source.Length)
            {
                cause = "ApplyTo patterns must use the canonical comma and space separator.";
                return false;
            }
        }

        if (values.Count == 0)
        {
            cause = "An applies to suffix requires at least one code span.";
            return false;
        }

        patterns = values;
        return true;
    }

    private static int FindFence(string source, int start, int fenceLength)
    {
        for (var index = start; index < source.Length;)
        {
            if (source[index] != '`')
            {
                index++;
                continue;
            }

            var run = CountRun(source, index, '`');
            if (run == fenceLength)
            {
                return index;
            }

            index += run;
        }

        return -1;
    }

    private static int FindApplyToSeparator(string source)
    {
        for (var index = 0; index < source.Length;)
        {
            if (source.AsSpan(index).StartsWith(ApplyToSeparator, StringComparison.Ordinal))
            {
                return index;
            }

            if (source[index] != '`')
            {
                index++;
                continue;
            }

            var fenceLength = CountRun(source, index, '`');
            var fenceEnd = FindFence(source, index + fenceLength, fenceLength);
            if (fenceEnd < 0)
            {
                return -1;
            }

            index = fenceEnd + fenceLength;
        }

        return -1;
    }

    private static int CountRun(string value, int start, char character)
    {
        var end = start;
        while (end < value.Length && value[end] == character)
        {
            end++;
        }

        return end - start;
    }

    private static string NormalizeCodeSpanContent(string value)
    {
        if (value.Length >= 2 && value[0] == ' ' && value[^1] == ' '
            && value.Any(character => character != ' '))
        {
            return value[1..^1];
        }

        return value;
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
