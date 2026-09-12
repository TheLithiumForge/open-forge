using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
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
            || selected[0].Value.Mapping is not { } members
            || IsFlowMapping(yaml.Source, selected[0].Value))
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
                Description = description?.Entry.Value.Scalar?.Value,
                Tags = parsedTags.ToImmutable(),
                Responsibility = responsibility?.Entry.Value.Scalar?.Value,
                DescriptionMember = description,
                ResponsibilityMember = responsibility,
                TagsMember = tags,
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

    private static RouteUpdateMetadataMemberLine? TryReadMemberLine(
        string source,
        string name,
        YamlMappingEntry entry)
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
            || !string.Equals(source[key.Span.End..value.Span.Start], ": ", StringComparison.Ordinal)
            || !string.IsNullOrWhiteSpace(source[value.Span.End..contentEnd]))
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
            Indentation = indentation,
            RawValue = rawValue,
        };
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
}
