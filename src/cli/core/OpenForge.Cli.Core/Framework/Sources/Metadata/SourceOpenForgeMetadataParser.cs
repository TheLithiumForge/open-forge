using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Framework.Sources.Metadata;

internal sealed class SourceOpenForgeMetadataParser
{
    private readonly YamlDocumentParser _yamlParser = new();

    internal SourceOpenForgeMetadataFacts Parse(MarkdownDocumentFacts document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Frontmatter.State == MarkdownFrontmatterState.Missing)
        {
            return SourceOpenForgeMetadataFacts.WithoutValues(SourceOpenForgeMetadataState.Missing);
        }

        if (document.Frontmatter.State != MarkdownFrontmatterState.Complete
            || document.Frontmatter.YamlSpan is not { } yamlSpan)
        {
            return SourceOpenForgeMetadataFacts.WithoutValues(SourceOpenForgeMetadataState.Malformed);
        }

        var yaml = document.Source[yamlSpan.Start..yamlSpan.End];
        var facts = _yamlParser.Parse(yaml);
        if (facts.State != YamlDocumentState.Complete
            || facts.HasAliases
            || facts.HasUnsupportedMappings)
        {
            return SourceOpenForgeMetadataFacts.WithoutValues(SourceOpenForgeMetadataState.Malformed);
        }

        if (facts.Root is not { } root
            || !root.TryGetMappingValue("open-forge", out var openForge)
            || openForge is null
            || !openForge.TryGetMappingValue("description", out var descriptionNode)
            || descriptionNode?.Scalar is not { } description
            || string.IsNullOrWhiteSpace(description.Value)
            || !openForge.TryGetMappingValue("tags", out var tagsNode)
            || tagsNode?.Sequence is not { Count: > 0 } tags
            || ReadTags(tags) is not { } tagValues)
        {
            return SourceOpenForgeMetadataFacts.WithoutValues(SourceOpenForgeMetadataState.Missing);
        }

        if (tagValues.Any(tag => !IsValidTag(tag)))
        {
            return SourceOpenForgeMetadataFacts.WithoutValues(SourceOpenForgeMetadataState.Malformed);
        }

        return SourceOpenForgeMetadataFacts.Complete(
            description.Value,
            tagValues);
    }

    private static IReadOnlyList<string>? ReadTags(IEnumerable<YamlNode> tags)
    {
        var values = new List<string>();
        foreach (var tag in tags)
        {
            if (tag.Scalar?.Value is not { } value)
            {
                return null;
            }

            values.Add(value);
        }

        return values;
    }

    internal static bool IsValidTag(string? tag)
    {
        if (string.IsNullOrEmpty(tag))
        {
            return false;
        }

        var runes = tag.EnumerateRunes().ToArray();
        if (runes.Length == 0 || !Rune.IsLetter(runes[0]))
        {
            return false;
        }

        var previousWasHyphen = false;
        for (var index = 1; index < runes.Length; index++)
        {
            var rune = runes[index];
            if (rune.Value == '-')
            {
                if (previousWasHyphen || index == runes.Length - 1)
                {
                    return false;
                }

                previousWasHyphen = true;
                continue;
            }

            if (!Rune.IsLetterOrDigit(rune))
            {
                return false;
            }

            previousWasHyphen = false;
        }

        return true;
    }
}
