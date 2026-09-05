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
            var duplicate = entries
                .Where(entry => string.Equals(entry.Key.Scalar?.Value, OpenForgeRoot, StringComparison.Ordinal))
                .Skip(1)
                .Select(entry => entry.Key.Scalar?.Span)
                .OfType<YamlTextSpan>()
                .First();
            return Duplicate(duplicate);
        }

        if (FrameworkDocumentMetadataValueReader.ReadDuplicateKey(selected[0]) is { } duplicateKey)
        {
            return Duplicate(duplicateKey);
        }

        if (!FrameworkDocumentMetadataValueReader.TryRead(
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

    private static FrameworkDocumentMetadataFacts Missing()
        => FrameworkDocumentMetadataFacts.WithoutValues(
            FrameworkDocumentMetadataState.Missing);

    private static FrameworkDocumentMetadataFacts Malformed()
        => FrameworkDocumentMetadataFacts.Malformed(
            FrameworkDocumentMetadataFailureKind.Malformed);

    private static FrameworkDocumentMetadataFacts Duplicate(YamlTextSpan span)
        => FrameworkDocumentMetadataFacts.Malformed(
            FrameworkDocumentMetadataFailureKind.Duplicate,
            span);

}
