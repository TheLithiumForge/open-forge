using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata;

internal sealed class FrameworkDocumentMetadataParser
{
    private const string OpenForgeRoot = "open-forge";
    private readonly YamlDocumentParser _yamlParser = new();

    internal FrameworkDocumentMetadataFacts Parse(MarkdownDocumentFacts document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Frontmatter.State == MarkdownFrontmatterState.Missing)
        {
            return Missing();
        }

        if (document.Frontmatter.State != MarkdownFrontmatterState.Complete
            || document.Frontmatter.YamlSpan is not { } yamlSpan)
        {
            return Malformed();
        }

        var yaml = document.Source[yamlSpan.Start..yamlSpan.End];
        var facts = _yamlParser.Parse(yaml);
        if (facts.State != YamlDocumentState.Complete)
        {
            return Malformed();
        }

        var root = facts.Root;
        if (root?.Mapping is not { } entries)
        {
            return Missing();
        }

        var selected = entries
            .Where(entry => string.Equals(
                entry.Key.Scalar?.Value,
                OpenForgeRoot,
                StringComparison.Ordinal))
            .Select(entry => entry.Value)
            .ToArray();
        if (selected.Length == 0)
        {
            return Missing();
        }

        if (selected.Length != 1)
        {
            return Malformed();
        }

        if (!TryReadMetadata(
                selected[0],
                out var metadata,
                out var tagSpans,
                out var malformed))
        {
            return malformed ? Malformed() : Missing();
        }

        return FrameworkDocumentMetadataFacts.Complete(
            metadata
                ?? throw new InvalidOperationException(
                    "Complete Framework document metadata parsing requires authored metadata."),
            tagSpans);
    }

    private static bool TryReadMetadata(
        YamlNode? node,
        out FrameworkDocumentMetadata? metadata,
        out IReadOnlyList<YamlTextSpan> tagSpans,
        out bool malformed)
    {
        metadata = null;
        tagSpans = [];
        malformed = false;
        if (node?.Kind == YamlNodeKind.Alias)
        {
            malformed = true;
            return false;
        }

        if (node?.Mapping is null)
        {
            return false;
        }

        var hasDescription = TryGetSupportedValue(
            node,
            "description",
            out var descriptionNode,
            out var duplicateDescription);
        var hasTags = TryGetSupportedValue(
            node,
            "tags",
            out var tagsNode,
            out var duplicateTags);
        var hasResponsibility = TryGetSupportedValue(
            node,
            "responsibility",
            out var responsibilityNode,
            out var duplicateResponsibility);
        if (duplicateDescription || duplicateTags || duplicateResponsibility)
        {
            malformed = true;
            return false;
        }

        if (hasDescription
            && (descriptionNode?.ContainsAlias == true || descriptionNode?.Scalar is null))
        {
            malformed = true;
            return false;
        }

        IReadOnlyList<YamlScalar>? tagFacts = null;
        if (hasTags)
        {
            if (tagsNode?.Sequence is not { } tags
                || ReadTags(tags) is not { } parsedTagFacts)
            {
                malformed = true;
                return false;
            }

            tagFacts = parsedTagFacts;
            if (tagFacts.Any(tag => !FrameworkDocumentMetadataTagGrammar.IsValid(tag.Value)))
            {
                malformed = true;
                return false;
            }
        }

        string? responsibility = null;
        if (hasResponsibility)
        {
            if (responsibilityNode?.ContainsAlias == true
                || responsibilityNode?.Scalar is not { } responsibilityScalar)
            {
                malformed = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(responsibilityScalar.Value))
            {
                return false;
            }

            responsibility = responsibilityScalar.Value;
        }

        if (!hasDescription
            || descriptionNode?.Scalar is not { } description
            || string.IsNullOrWhiteSpace(description.Value)
            || !hasTags
            || tagFacts is not { Count: > 0 })
        {
            return false;
        }

        var tagValues = tagFacts.Select(tag => tag.Value).ToArray();
        metadata = new FrameworkDocumentMetadata(
            description.Value,
            tagValues,
            responsibility);
        tagSpans = tagFacts.Select(tag => tag.Span).ToArray();
        return true;
    }

    private static IReadOnlyList<YamlScalar>? ReadTags(IEnumerable<YamlNode> tags)
    {
        var values = new List<YamlScalar>();
        foreach (var tag in tags)
        {
            if (tag.Scalar is not { } scalar)
            {
                return null;
            }

            values.Add(scalar);
        }

        return values;
    }

    private static bool TryGetSupportedValue(
        YamlNode node,
        string key,
        out YamlNode? value,
        out bool duplicate)
    {
        value = null;
        duplicate = false;
        foreach (var entry in node.Mapping
                     ?? throw new InvalidOperationException(
                         "Open Forge metadata value selection requires a mapping node."))
        {
            if (!string.Equals(entry.Key.Scalar?.Value, key, StringComparison.Ordinal))
            {
                continue;
            }

            if (value is not null)
            {
                duplicate = true;
                value = null;
                return true;
            }

            value = entry.Value;
        }

        return value is not null;
    }

    private static FrameworkDocumentMetadataFacts Missing()
        => FrameworkDocumentMetadataFacts.WithoutValues(
            FrameworkDocumentMetadataState.Missing);

    private static FrameworkDocumentMetadataFacts Malformed()
        => FrameworkDocumentMetadataFacts.WithoutValues(
            FrameworkDocumentMetadataState.Malformed);
}
