using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning.Metadata;

internal sealed partial class RouteUpdateMetadataLayoutReader
{
    private static RouteUpdateMetadataMemberLine? TryReadApplyToMemberLine(
        string source,
        YamlMappingEntry entry)
    {
        if (entry.Value.Sequence is { } sequence
            && ContainsCommentInsideSequence(
                source,
                entry.Value,
                sequence,
                includeTrailingTrivia: IsFlowSequence(source, entry.Value)))
        {
            return null;
        }

        var singleLine = TryReadMemberLine(
            source,
            "applyTo",
            entry,
            allowTrailingComment: true);
        if (singleLine is not null)
        {
            return singleLine;
        }

        if (entry.Value.Sequence is not { Count: > 0 } items
            || entry.Key.Scalar is null
            || !string.Equals(
                source[entry.Key.Span.Start..entry.Key.Span.End],
                "applyTo",
                StringComparison.Ordinal)
            || entry.Value.Span.Length == 0
            || entry.Value.Span.End > source.Length)
        {
            return null;
        }

        var key = entry.Key;
        var value = entry.Value;
        var lineStart = ReadLineStart(source, key.Span.Start);
        var contentEnd = ReadLineContentEnd(source, key.Span.Start);
        var lineEnd = ReadLineEnd(source, contentEnd);
        var valueLineStart = ReadLineStart(source, value.Span.Start);
        if (key.Span.End >= source.Length || source[key.Span.End] != ':')
        {
            return null;
        }

        if (value.Span.Start < contentEnd)
        {
            if (!string.Equals(
                    source[key.Span.End..value.Span.Start],
                    ": ",
                    StringComparison.Ordinal))
            {
                return null;
            }
        }
        else if (valueLineStart < lineEnd
            || !IsWhitespaceOrComment(source[(key.Span.End + 1)..contentEnd])
            || !IsBlankOrCommentLines(source, lineEnd, value.Span.Start))
        {
            return null;
        }

        var indentation = source[lineStart..key.Span.Start];
        if (indentation.Any(character => character is not (' ' or '\t')))
        {
            return null;
        }

        var isFlowSequence = IsFlowSequence(source, value);
        var valueStart = value.Span.Start;
        var valueEnd = isFlowSequence ? value.Span.End : items[^1].Span.End;
        if (valueEnd <= valueStart || valueEnd > source.Length)
        {
            return null;
        }

        var lastSyntaxPosition = valueEnd - 1;
        var fieldEnd = ReadLineEnd(
            source,
            ReadLineContentEnd(source, lastSyntaxPosition));
        if (fieldEnd < valueEnd)
        {
            return null;
        }

        var hasCommentOutsideValue = source[(key.Span.End + 1)..value.Span.Start].Contains('#')
            || source[valueEnd..ReadLineContentEnd(source, valueEnd)].Contains('#');
        return new RouteUpdateMetadataMemberLine
        {
            Start = lineStart,
            ContentEnd = contentEnd,
            End = fieldEnd,
            ValueStart = valueStart,
            ValueEnd = valueEnd,
            Indentation = indentation,
            RawValue = source[valueStart..valueEnd],
            HasTrailingComment = hasCommentOutsideValue,
            IsMultilineValue = true,
        };
    }

    private static bool ContainsCommentInsideSequence(
        string source,
        YamlNode value,
        IReadOnlyList<YamlNode> items,
        bool includeTrailingTrivia)
    {
        var cursor = value.Span.Start;
        var ordered = items.OrderBy(item => item.Span.Start).ToArray();
        for (var index = 0; index < ordered.Length; index++)
        {
            var item = ordered[index];
            if (item.Span.Start < cursor
                || item.Span.End < item.Span.Start
                || item.Span.End > value.Span.End
                || value.Span.End > source.Length)
            {
                return true;
            }

            if (source[cursor..item.Span.Start].Contains('#'))
            {
                return true;
            }

            cursor = item.Span.End;
        }

        return includeTrailingTrivia
            && source[cursor..value.Span.End].Contains('#');
    }

    private static bool IsWhitespaceOrComment(string value)
    {
        var trimmed = value.AsSpan().TrimStart();
        return trimmed.IsEmpty || trimmed[0] == '#';
    }

    private static bool IsBlankOrCommentLines(
        string source,
        int start,
        int end)
    {
        var endLineStart = ReadLineStart(source, end);
        if (endLineStart >= start)
        {
            var prefix = source[endLineStart..end].AsSpan().TrimStart();
            if (!prefix.IsEmpty)
            {
                return false;
            }

            end = endLineStart;
        }

        var lineStart = start;
        while (lineStart < end)
        {
            var contentEnd = ReadLineContentEnd(source, lineStart);
            var trimmed = source[lineStart..contentEnd].AsSpan().TrimStart();
            if (!trimmed.IsEmpty && trimmed[0] != '#')
            {
                return false;
            }

            var lineEnd = ReadLineEnd(source, contentEnd);
            if (lineEnd <= lineStart)
            {
                break;
            }

            lineStart = lineEnd;
        }

        return true;
    }

    private static RouteUpdateMetadataMemberLine? TryReadMemberLine(
        string source,
        string name,
        YamlMappingEntry entry,
        bool allowTrailingComment = false,
        bool preserveRootTrivia = false)
    {
        var key = entry.Key;
        var value = entry.Value;
        if (key.Scalar is null
            || !string.Equals(key.Scalar.Value, name, StringComparison.Ordinal)
            || !preserveRootTrivia && !string.Equals(source[key.Span.Start..key.Span.End], name, StringComparison.Ordinal))
        {
            return null;
        }

        var lineStart = ReadLineStart(source, key.Span.Start);
        var contentEnd = ReadLineContentEnd(source, key.Span.Start);
        var lineEnd = ReadLineEnd(source, contentEnd);
        var separator = source[key.Span.End..value.Span.Start];
        var rootSeparator = separator.AsSpan().TrimStart();
        var safeSeparator = preserveRootTrivia
            ? rootSeparator.Length > 0 && rootSeparator[0] == ':' && rootSeparator[1..].Trim().IsEmpty
            : string.Equals(separator, ": ", StringComparison.Ordinal);
        if (value.Span.End > contentEnd || !safeSeparator)
        {
            return null;
        }

        var suffix = source[value.Span.End..contentEnd].AsSpan().TrimStart();
        if (!suffix.IsEmpty
            && (!allowTrailingComment || suffix[0] != '#'))
        {
            return null;
        }

        var rawValue = source[value.Span.Start..value.Span.End];
        var trimmed = rawValue.AsSpan().TrimStart();
        if (trimmed.StartsWith("&", StringComparison.Ordinal)
            || trimmed.StartsWith("!", StringComparison.Ordinal))
        {
            return null;
        }

        var indentation = source[lineStart..key.Span.Start];
        if (indentation.Any(character => character is not (' ' or '\t')))
        {
            return null;
        }

        return new RouteUpdateMetadataMemberLine
        {
            Start = lineStart,
            ContentEnd = contentEnd,
            End = lineEnd,
            ValueStart = value.Span.Start,
            ValueEnd = value.Span.End,
            Indentation = indentation,
            RawValue = rawValue,
            HasTrailingComment = !suffix.IsEmpty,
        };
    }

    private static YamlMappingEntry? ReadDeclarationEntry(
        YamlDocumentFacts? yaml,
        FrameworkDocumentMetadataSyntax syntax,
        ApplyToDeclaration declaration)
    {
        var mapping = declaration.Location switch
        {
            ApplyToMetadataLocation.Root => yaml?.Root?.Mapping,
            ApplyToMetadataLocation.OpenForge => syntax.ScopedEntry?.Value.Mapping,
            _ => throw new ArgumentOutOfRangeException(nameof(declaration), declaration.Location,
                "The applyTo declaration location is not defined."),
        };
        return mapping?.SingleOrDefault(entry => entry.Key.Span == declaration.KeySpan
            && entry.Value.Span == declaration.ValueSpan);
    }

    private static int ReadLineStart(string source, int position)
    {
        var index = position;
        while (index > 0 && source[index - 1] is not ('\r' or '\n'))
        {
            index--;
        }

        return index;
    }

    private static int ReadLineContentEnd(string source, int position)
    {
        var index = position;
        while (index < source.Length && source[index] is not ('\r' or '\n'))
        {
            index++;
        }

        return index;
    }

    private static int ReadLineEnd(string source, int contentEnd)
    {
        if (contentEnd >= source.Length)
        {
            return contentEnd;
        }

        if (source[contentEnd] == '\r'
            && contentEnd + 1 < source.Length
            && source[contentEnd + 1] == '\n')
        {
            return contentEnd + 2;
        }

        return contentEnd + 1;
    }

    private static bool IsFlowMapping(string source, YamlNode value)
        => source.AsSpan(value.Span.Start, value.Span.Length).TrimStart()
            .StartsWith("{", StringComparison.Ordinal);

    private static bool IsFlowSequence(string source, YamlNode value)
        => value.Span.Start < source.Length
            && source[value.Span.Start] == '[';

    private static bool IsEmptyFlowMapping(string source, YamlNode value)
        => string.Equals(
            source[value.Span.Start..value.Span.End],
            "{}",
            StringComparison.Ordinal);

    private static RouteUpdateMetadataEmptyMapping? ReadEmptyMapping(
        string source,
        YamlMappingEntry entry)
    {
        var line = TryReadMemberLine(source, "open-forge", entry);
        if (line is null
            || line.HasTrailingComment
            || !string.Equals(line.RawValue, "{}", StringComparison.Ordinal))
        {
            return null;
        }

        return new RouteUpdateMetadataEmptyMapping
        {
            Span = entry.Value.Span,
            Line = line,
        };
    }
}
