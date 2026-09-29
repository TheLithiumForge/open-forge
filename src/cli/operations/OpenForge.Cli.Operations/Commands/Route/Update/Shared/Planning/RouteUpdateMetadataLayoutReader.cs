using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed class RouteUpdateMetadataLayoutReader
{
    internal RouteUpdateMetadataLayoutRead Read(RouteUpdateObservation observation)
    {
        var yaml = observation.Frontmatter;
        if (observation.Markdown.Frontmatter.State != MarkdownFrontmatterState.Complete
            || observation.Markdown.Frontmatter.YamlSpan is null
            || yaml.State != YamlDocumentState.Complete
            || yaml.Root?.Mapping is not { } root
            || yaml.HasAliases)
        {
            return RouteUpdateMetadataLayoutRead.Unsafe(
                "The target frontmatter does not expose one safe parsed YAML mapping.");
        }

        var selected = root
            .Where(entry => string.Equals(
                entry.Key.Scalar?.Value,
                "open-forge",
                StringComparison.Ordinal))
            .ToArray();
        if (selected.Length != 1
            || selected[0].Value.Mapping is not { } members)
        {
            return RouteUpdateMetadataLayoutRead.Unsafe(
                "The target requires one block-form Open Forge metadata mapping.");
        }

        var selectedValue = selected[0].Value;
        var isFlowMapping = IsFlowMapping(yaml.Source, selectedValue);
        var emptyMapping = members.Count == 0 && IsEmptyFlowMapping(yaml.Source, selectedValue)
            ? ReadEmptyMapping(yaml.Source, selected[0])
            : null;
        if (isFlowMapping && emptyMapping is null)
        {
            return RouteUpdateMetadataLayoutRead.Unsafe(
                "The target requires one block-form Open Forge metadata mapping.");
        }

        var recognized = ReadRecognizedMembers(members);
        if (recognized.Values.Any(values => values.Count != 1))
        {
            return RouteUpdateMetadataLayoutRead.Unsafe(
                "Recognized Open Forge metadata keys must occur exactly once.");
        }

        var description = ReadMember(yaml.Source, recognized, "description");
        var responsibility = ReadMember(yaml.Source, recognized, "responsibility");
        var tags = ReadMember(yaml.Source, recognized, "tags");
        var applyTo = ApplyToMetadataReader.Read(yaml);
        if (applyTo.State == ApplyToMetadataState.Invalid)
        {
            return RouteUpdateMetadataLayoutRead.Unsafe(
                "The target applyTo metadata is invalid or ambiguous.");
        }

        var applyToMembers = ImmutableArray.CreateBuilder<RouteUpdateMetadataMember>();
        foreach (var declaration in applyTo.Declarations)
        {
            var entry = FindApplyToEntry(root, selectedValue, declaration);
            var line = entry is null
                ? null
                : TryReadApplyToMemberLine(yaml.Source, entry);
            if (entry is null || line is null)
            {
                return RouteUpdateMetadataLayoutRead.Unsafe(
                    "The target applyTo declaration cannot be edited without reserializing unrelated YAML.");
            }

            applyToMembers.Add(new RouteUpdateMetadataMember
            {
                Name = "applyTo",
                Entry = entry,
                Line = line,
            });
        }

        if (description is not null && description.Entry.Value.Scalar is null
            || tags is not null && tags.Entry.Value.Sequence is null)
        {
            return RouteUpdateMetadataLayoutRead.Unsafe(
                "The target description and tags require supported parsed scalar forms.");
        }

        if (responsibility is not null && responsibility.Entry.Value.Scalar is null)
        {
            return RouteUpdateMetadataLayoutRead.Unsafe(
                "The target responsibility requires one supported parsed scalar form.");
        }

        var parsedTags = ImmutableArray.CreateBuilder<string>();
        foreach (var node in tags?.Entry.Value.Sequence ?? [])
        {
            if (node.Scalar?.Value is not { } value)
            {
                return RouteUpdateMetadataLayoutRead.Unsafe(
                    "The target tags require supported parsed scalar forms.");
            }

            parsedTags.Add(value);
        }

        return RouteUpdateMetadataLayoutRead.Complete(
            new RouteUpdateMetadataLayout
            {
                Source = yaml.Source,
                EmptyMapping = emptyMapping,
                Description = description?.Entry.Value.Scalar?.Value,
                Tags = parsedTags.ToImmutable(),
                Responsibility = responsibility?.Entry.Value.Scalar?.Value,
                DescriptionMember = description,
                ResponsibilityMember = responsibility,
                TagsMember = tags,
                ApplyToPatterns = applyTo.Patterns,
                ApplyToMembers = applyToMembers.ToImmutable(),
            });
    }

    private static Dictionary<string, List<YamlMappingEntry>> ReadRecognizedMembers(
        IReadOnlyList<YamlMappingEntry> members)
    {
        var recognized = new Dictionary<string, List<YamlMappingEntry>>(StringComparer.Ordinal);
        foreach (var entry in members)
        {
            var name = entry.Key.Scalar?.Value;
            if (name is not ("description" or "responsibility" or "tags"))
            {
                continue;
            }

            if (!recognized.TryGetValue(name, out var entries))
            {
                entries = [];
                recognized.Add(name, entries);
            }

            entries.Add(entry);
        }

        return recognized;
    }

    private static RouteUpdateMetadataMember? ReadMember(
        string source,
        IReadOnlyDictionary<string, List<YamlMappingEntry>> recognized,
        string name)
    {
        if (!recognized.TryGetValue(name, out var values))
        {
            return null;
        }

        var entry = values[0];
        return new RouteUpdateMetadataMember
        {
            Name = name,
            Entry = entry,
            Line = TryReadMemberLine(source, name, entry),
        };
    }

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
        bool allowTrailingComment = false)
    {
        var key = entry.Key;
        var value = entry.Value;
        if (key.Scalar is null
            || !string.Equals(source[key.Span.Start..key.Span.End], name, StringComparison.Ordinal))
        {
            return null;
        }

        var lineStart = ReadLineStart(source, key.Span.Start);
        var contentEnd = ReadLineContentEnd(source, key.Span.Start);
        var lineEnd = ReadLineEnd(source, contentEnd);
        if (value.Span.End > contentEnd
            || !string.Equals(source[key.Span.End..value.Span.Start], ": ", StringComparison.Ordinal))
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

    private static YamlMappingEntry? FindApplyToEntry(
        IReadOnlyList<YamlMappingEntry> root,
        YamlNode openForge,
        ApplyToDeclaration declaration)
    {
        var mapping = declaration.Location switch
        {
            ApplyToMetadataLocation.Root => root,
            ApplyToMetadataLocation.OpenForge => openForge.Mapping,
            _ => throw new ArgumentOutOfRangeException(
                nameof(declaration),
                declaration.Location,
                "The applyTo declaration location is not defined."),
        };

        return mapping?.SingleOrDefault(entry =>
            entry.Key.Span == declaration.KeySpan
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
