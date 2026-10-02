using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Shared;

internal sealed class WorkspaceAdoptionDocumentPlanner
{
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly YamlDocumentParser _yamlParser = new();
    private readonly SourceAuthoredMetadataParser _authoredMetadataParser = new();
    private readonly SourceOpenForgeMetadataParser _openForgeMetadataParser = new();
    private readonly WorkspaceAdoptionSkillMetadataPlanner _skillMetadataPlanner = new();
    private readonly WorkspaceAdoptionEntrypointComposer _entrypointComposer = new();

    internal WorkspaceAdoptionDocumentPlan Plan(
        string canonicalPath,
        SourceDocumentForm form,
        ReadOnlyMemory<byte> originalBytes,
        bool ensureEntries)
    {
        if (canonicalPath is null || !SourceLogicalPath.IsCanonicalSource(canonicalPath))
        {
            return Blocked(canonicalPath ?? string.Empty, "field 'canonicalPath' must identify a canonical source.");
        }

        if (!Enum.IsDefined(form) || form == SourceDocumentForm.OverwriteCompanion)
        {
            return Blocked(canonicalPath, "field 'form' must identify a supported logical source form.");
        }

        var isEntrypoint = IsEntrypoint(form);
        if (form == SourceDocumentForm.Skill && ensureEntries)
        {
            return Blocked(canonicalPath, "field 'Entries' cannot be ensured for a native Skill.");
        }

        if (ensureEntries && !isEntrypoint)
        {
            return Blocked(canonicalPath, "field 'Entries' can only be ensured for an entrypoint form.");
        }

        if (!WorkspaceAdoptionDocumentText.TryDecode(originalBytes, out var source)
            || source.Contains('\0'))
        {
            return Blocked(canonicalPath, "field 'encoding' is not supported strict UTF-8.");
        }

        var markdown = _markdownParser.Parse(source);
        if (markdown.Frontmatter.State == MarkdownFrontmatterState.Unavailable)
        {
            return Blocked(canonicalPath, "field 'frontmatter' has no safe closing boundary.");
        }

        if (form == SourceDocumentForm.Skill)
        {
            var authoredSkill = _authoredMetadataParser.Parse(markdown, form);
            return _skillMetadataPlanner.Plan(
                canonicalPath,
                source,
                markdown,
                authoredSkill,
                originalBytes);
        }

        var authored = _authoredMetadataParser.Parse(markdown, form);
        var openForge = _openForgeMetadataParser.Parse(markdown);
        var applyTo = form == SourceDocumentForm.Loader ? openForge.ApplyTo : authored.ApplyTo;
        if (applyTo.State == ApplyToMetadataState.Invalid)
        {
            return Blocked(canonicalPath, "field 'applyTo' is invalid.");
        }

        if (openForge.State == SourceOpenForgeMetadataState.Malformed)
        {
            return Blocked(canonicalPath, $"field '{ResolveMalformedMetadataField(markdown)}' is malformed.");
        }

        if (!ensureEntries || markdown.GeneratedRegion.State == MarkdownGeneratedRegionState.Complete)
        {
            return Unchanged(originalBytes);
        }

        if (markdown.GeneratedRegion.State != MarkdownGeneratedRegionState.Absent)
        {
            var detail = markdown.GeneratedRegion.Cause;
            return Blocked(
                canonicalPath,
                string.IsNullOrWhiteSpace(detail)
                    ? "field 'Entries' has an unsafe or ambiguous section boundary."
                    : $"field 'Entries' has an unsafe or ambiguous section boundary ({detail}).");
        }

        var updated = WorkspaceAdoptionDocumentText.AppendEntriesSection(source);
        var updatedMarkdown = _markdownParser.Parse(updated);
        if (updatedMarkdown.GeneratedRegion.State != MarkdownGeneratedRegionState.Complete)
        {
            return Blocked(canonicalPath, "field 'Entries' could not be added at a safe top-level boundary.");
        }

        if (!WorkspaceAdoptionDocumentText.TryEncode(updated, out var intendedBytes))
        {
            return Blocked(canonicalPath, "field 'encoding' cannot represent the appended Entries section as strict UTF-8.");
        }

        return new WorkspaceAdoptionDocumentPlan(
            intendedBytes,
            [WorkspaceAdoptionAction.EntriesSectionAdded],
            [],
            [],
            cause: null);
    }

    internal WorkspaceAdoptionDocumentPlan CreateEntrypoint(string canonicalPath, string newline)
        => _entrypointComposer.Create(canonicalPath, newline);

    private static bool IsEntrypoint(SourceDocumentForm form)
        => form is SourceDocumentForm.CanonicalEntrypoint
            or SourceDocumentForm.IndexEntrypoint
            or SourceDocumentForm.UnderscoreIndexEntrypoint
            or SourceDocumentForm.ReferencesEntrypoint
            or SourceDocumentForm.UnderscoreReferencesEntrypoint;

    private string ResolveMalformedMetadataField(
        OpenForge.Cli.Core.Framework.Documents.Markdown.Models.MarkdownDocumentFacts markdown)
    {
        if (markdown.Frontmatter.YamlSpan is not { } yamlSpan)
        {
            return "frontmatter";
        }

        var yaml = markdown.Source[yamlSpan.Start..yamlSpan.End];
        var parsed = _yamlParser.Parse(yaml);
        if (parsed.State != YamlDocumentState.Complete || parsed.Root?.Mapping is not { } mapping)
        {
            return "frontmatter";
        }

        return mapping.Any(entry => string.Equals(
                entry.Key.Scalar?.Value,
                "open-forge",
                StringComparison.Ordinal))
            ? "open-forge"
            : "frontmatter";
    }

    private static WorkspaceAdoptionDocumentPlan Unchanged(ReadOnlyMemory<byte> originalBytes)
        => new(
            originalBytes.ToArray(),
            actions: [],
            fields: [],
            derivation: [],
            cause: null);

    private static WorkspaceAdoptionDocumentPlan Blocked(string path, string reason)
        => new(
            intendedBytes: null,
            actions: [],
            fields: [],
            derivation: [],
            cause: $"{path}: {reason}");
}
