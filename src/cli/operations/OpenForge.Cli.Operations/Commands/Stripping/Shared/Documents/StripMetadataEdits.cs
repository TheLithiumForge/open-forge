using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Reading;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Stripping.Shared.Documents;

/// <summary>
/// Removes Open Forge frontmatter keys by whole lines and keeps every other byte.
/// The strip Interface contract's metadata part defines which keys go.
/// </summary>
internal static class StripMetadataEdits
{
    internal static StripDocumentEdits Read(string path, MarkdownDocumentFacts facts)
    {
        if (SourceLogicalPath.ReadFileName(path) == SourceFormClassifier.NativeSkillFileName)
        {
            return StripDocumentEdits.None;
        }

        var frontmatter = facts.Frontmatter;
        if (frontmatter.State == MarkdownFrontmatterState.Missing)
        {
            return StripDocumentEdits.None;
        }

        if (frontmatter is not { State: MarkdownFrontmatterState.Complete, BlockSpan: { } block, YamlSpan: { } yamlSpan, BodyStart: { } bodyStart })
        {
            return StripDocumentEdits.Refuse("The frontmatter has no closing --- line.");
        }

        if (facts.FrontmatterSyntax is not { State: YamlDocumentState.Complete } yaml)
        {
            return StripDocumentEdits.Refuse("The frontmatter is not valid YAML.");
        }

        if (yaml.Root?.Mapping is not { } entries)
        {
            return StripDocumentEdits.None;
        }

        var removed = SelectRemoved(entries, out var duplicate);
        if (duplicate)
        {
            return StripDocumentEdits.Refuse("The frontmatter has more than one open-forge key.");
        }

        if (removed.Count == 0)
        {
            return StripDocumentEdits.None;
        }

        if (removed.Count == entries.Count)
        {
            var start = facts.Source.Length > 0 && facts.Source[0] == '﻿' ? 1 : block.Start;
            return new StripDocumentEdits([new StripTextEdit(start, bodyStart, string.Empty, StripRewritePart.Metadata)]);
        }

        var edits = new List<StripTextEdit>();
        var kept = entries.Where(entry => !removed.Contains(entry)).Select(entry => yamlSpan.Start + entry.Key.Span.Start).ToArray();
        foreach (var entry in removed)
        {
            var keyStart = yamlSpan.Start + entry.Key.Span.Start;
            var lineStart = StripSourceLines.LineStart(facts.Source, keyStart);
            if (!StripSourceLines.IsBlank(facts.Source, lineStart, keyStart))
            {
                return StripDocumentEdits.Refuse("An Open Forge key shares its line with other YAML.");
            }

            var last = yamlSpan.Start + Math.Max(entry.Key.Span.End, entry.Value.Span.End) - 1;
            var end = StripSourceLines.NextLineStart(facts.Source, Math.Max(last, keyStart));
            if (kept.Any(position => position >= lineStart && position < end))
            {
                return StripDocumentEdits.Refuse("An Open Forge key shares its lines with another key.");
            }

            edits.Add(new StripTextEdit(lineStart, Math.Min(end, yamlSpan.End), string.Empty, StripRewritePart.Metadata));
        }

        return new StripDocumentEdits(edits);
    }

    private static HashSet<YamlMappingEntry> SelectRemoved(IReadOnlyList<YamlMappingEntry> entries, out bool duplicate)
    {
        var scoped = entries.Where(entry => IsKey(entry, ApplyToMetadataReader.OpenForgeKey)).ToArray();
        duplicate = scoped.Length > 1;
        var removed = new HashSet<YamlMappingEntry>(ReferenceEqualityComparer.Instance);
        if (scoped.Length == 1)
        {
            removed.Add(scoped[0]);
            foreach (var entry in entries.Where(entry => IsKey(entry, ApplyToMetadataReader.ApplyToKey)))
            {
                removed.Add(entry);
            }

            return removed;
        }

        foreach (var entry in entries.Where(entry => IsKey(entry, ApplyToMetadataReader.ApplyToKey)
            || FrameworkMetadataFieldReader.Read(entry.Key.Scalar?.Value) is not null))
        {
            removed.Add(entry);
        }

        return removed;
    }

    private static bool IsKey(YamlMappingEntry entry, string key)
        => string.Equals(entry.Key.Scalar?.Value, key, StringComparison.Ordinal);
}
