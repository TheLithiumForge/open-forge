using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata;

internal static class FrameworkDocumentMetadataValueReader
{
    internal static bool TryRead(
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
            if (tagsNode?.Sequence is not { } tags || ReadTags(tags) is not { } parsedTags)
            {
                malformed = true;
                return false;
            }

            tagFacts = parsedTags;
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

        metadata = new FrameworkDocumentMetadata(
            description.Value,
            tagFacts.Select(tag => tag.Value).ToArray(),
            responsibility);
        tagSpans = tagFacts.Select(tag => tag.Span).ToArray();
        return true;
    }

    internal static YamlTextSpan? ReadDuplicateKey(YamlNode node)
    {
        if (node.Mapping is not { } mapping)
        {
            return null;
        }

        var supported = new HashSet<string>(
            ["description", "tags", "responsibility"],
            StringComparer.Ordinal);
        return mapping
            .Where(entry => entry.Key.Scalar is { } scalar && supported.Contains(scalar.Value))
            .GroupBy(entry => entry.Key.Scalar?.Value, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group.Skip(1))
            .OrderBy(entry => entry.Key.Scalar?.Span.Start)
            .Select(entry => entry.Key.Scalar?.Span)
            .OfType<YamlTextSpan>()
            .FirstOrDefault();
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
}
