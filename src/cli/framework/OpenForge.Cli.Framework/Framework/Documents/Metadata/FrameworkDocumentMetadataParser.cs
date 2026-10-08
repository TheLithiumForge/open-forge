using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Reading;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata;

internal sealed class FrameworkDocumentMetadataParser
{
    private const string OpenForgeRoot = "open-forge";
    private readonly YamlDocumentParser _yamlParser = new();

    internal FrameworkDocumentMetadataFacts Parse(MarkdownDocumentFacts document, FrameworkMetadataReadScope scope)
    {
        ArgumentNullException.ThrowIfNull(document);
        ValidateScope(scope);
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
        return Read(_yamlParser.Parse(yaml), scope);
    }

    internal FrameworkDocumentMetadataFacts Read(YamlDocumentFacts facts, FrameworkMetadataReadScope scope)
    {
        ArgumentNullException.ThrowIfNull(facts);
        ValidateScope(scope);
        if (facts.State != YamlDocumentState.Complete)
        {
            return Malformed();
        }

        var applyTo = ApplyToMetadataReader.Read(facts);

        var root = facts.Root;
        if (root?.Mapping is not { } entries)
        {
            return Missing() with { ApplyTo = applyTo };
        }

        var scopedEntries = entries
            .Where(entry => string.Equals(
                entry.Key.Scalar?.Value,
                OpenForgeRoot,
                StringComparison.Ordinal))
            .ToArray();
        var scopedEntry = scopedEntries.FirstOrDefault();
        var selected = scopedEntry?.Value;
        if (scopedEntry is null && scope == FrameworkMetadataReadScope.RoutedSource)
        {
            selected = root;
        }

        var syntax = ReadSyntax(root, selected, scopedEntry);
        if (scopedEntries.Length > 1)
        {
            return Duplicate(scopedEntries[1].Key.Span) with { ApplyTo = applyTo, Syntax = syntax };
        }

        if (selected is null)
        {
            return Missing() with { ApplyTo = applyTo, Syntax = syntax };
        }

        if (FrameworkDocumentMetadataValueReader.ReadDuplicateKey(selected) is { } duplicateKey)
        {
            return Duplicate(duplicateKey) with { ApplyTo = applyTo, Syntax = syntax };
        }

        if (!FrameworkDocumentMetadataValueReader.TryRead(
                selected,
                out var metadata,
                out var tagSpans,
                out var observedDescription,
                out var observedTags,
                out var malformed))
        {
            return malformed
                ? Malformed(observedDescription) with { ApplyTo = applyTo, Syntax = syntax }
                : Missing(observedDescription, observedTags) with { ApplyTo = applyTo, Syntax = syntax };
        }

        var authoredMetadata = metadata
            ?? throw new InvalidOperationException(
                "Complete Framework document metadata parsing requires authored metadata.");
        var completeMetadata = new FrameworkDocumentMetadata(
            authoredMetadata.Description,
            authoredMetadata.Tags,
            authoredMetadata.Responsibility,
            applyTo.Patterns);
        return FrameworkDocumentMetadataFacts.Complete(completeMetadata, tagSpans) with { ApplyTo = applyTo, Syntax = syntax };
    }

    private static FrameworkDocumentMetadataSyntax ReadSyntax(
        YamlNode root,
        YamlNode? selected,
        YamlMappingEntry? scopedEntry)
    {
        var form = scopedEntry is null ? FrontmatterForm.Root : FrontmatterForm.Scoped;
        var members = ImmutableArray.CreateBuilder<FrameworkMetadataMemberSyntax>();
        foreach (var entry in selected?.Mapping ?? [])
        {
            if (FrameworkMetadataFieldReader.Read(entry.Key.Scalar?.Value) is not { } field)
            {
                continue;
            }

            members.Add(new FrameworkMetadataMemberSyntax
            {
                Field = field,
                Form = form,
                Entry = entry,
                TagItemSpans = field == FrameworkMetadataField.Tags
                    ? [.. (entry.Value.Sequence ?? []).Select(item => item.Span)]
                    : [],
            });
        }

        return new FrameworkDocumentMetadataSyntax
        {
            AuthoredForm = scopedEntry is not null || root.Mapping?.Any(entry => FrameworkMetadataFieldReader.Read(entry.Key.Scalar?.Value) is not null) == true
                ? form
                : null,
            ScopedEntry = scopedEntry,
            SelectedMapping = selected?.Kind == YamlNodeKind.Mapping ? selected : null,
            Members = members.ToImmutable(),
        };
    }

    private static void ValidateScope(FrameworkMetadataReadScope scope)
    {
        if (!Enum.IsDefined(scope))
        {
            throw new ArgumentOutOfRangeException(nameof(scope), scope, "The Framework metadata read scope is not defined.");
        }
    }

    private static FrameworkDocumentMetadataFacts Missing(
        string? observedDescription = null,
        IEnumerable<string>? observedTags = null)
        => FrameworkDocumentMetadataFacts.WithoutValues(
            FrameworkDocumentMetadataState.Missing,
            observedDescription,
            observedTags);

    private static FrameworkDocumentMetadataFacts Malformed(string? observedDescription = null)
        => FrameworkDocumentMetadataFacts.Malformed(
            FrameworkDocumentMetadataFailureKind.Malformed,
            observedDescription: observedDescription);

    private static FrameworkDocumentMetadataFacts Duplicate(YamlTextSpan span)
        => FrameworkDocumentMetadataFacts.Malformed(
            FrameworkDocumentMetadataFailureKind.Duplicate,
            span);

}
