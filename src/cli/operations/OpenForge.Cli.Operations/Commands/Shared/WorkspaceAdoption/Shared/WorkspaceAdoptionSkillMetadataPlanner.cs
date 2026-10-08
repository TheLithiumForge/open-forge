using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using YamlDotNet.Core;

namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Shared;

internal sealed class WorkspaceAdoptionSkillMetadataPlanner
{
    private const string NameField = "name";
    private const string DescriptionField = "description";
    private const string TitleField = "title";
    private const string OpenForgeField = "open-forge";

    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly YamlDocumentParser _yamlParser = new();
    private readonly SourceAuthoredMetadataParser _authoredMetadataParser = new();
    private readonly SourceOpenForgeMetadataParser _openForgeMetadataParser = new();

    internal WorkspaceAdoptionDocumentPlan Plan(
        string canonicalPath,
        string source,
        MarkdownDocumentFacts markdown,
        SourceAuthoredMetadataFacts authored,
        ReadOnlyMemory<byte> originalBytes)
    {
        if (markdown.Frontmatter.State == MarkdownFrontmatterState.Unavailable)
        {
            return Blocked(canonicalPath, "field 'frontmatter' has no safe closing boundary.");
        }

        if (authored.ApplyTo.State == ApplyToMetadataState.Invalid)
        {
            return Blocked(canonicalPath, "field 'applyTo' is invalid.");
        }

        if (authored.State == SourceAuthoredMetadataState.Complete)
        {
            return Unchanged(originalBytes);
        }

        if (markdown.Frontmatter.State == MarkdownFrontmatterState.Missing)
        {
            return AddMissingFrontmatter(canonicalPath, source, markdown);
        }

        if (markdown.Frontmatter.YamlSpan is not { } yamlSpan)
        {
            return Blocked(canonicalPath, "field 'frontmatter' has no safe YAML span.");
        }

        var yaml = source[yamlSpan.Start..yamlSpan.End];
        var syntax = _yamlParser.Parse(yaml);
        if (syntax.State != YamlDocumentState.Complete)
        {
            return Blocked(canonicalPath, "field 'frontmatter' contains invalid YAML.");
        }

        var openForge = _openForgeMetadataParser.Parse(markdown, FrameworkMetadataReadScope.ScopedOnly);
        if (openForge.State == SourceOpenForgeMetadataState.Malformed)
        {
            var containsOpenForge = syntax.Root?.Mapping?.Any(entry => string.Equals(
                entry.Key.Scalar?.Value,
                OpenForgeField,
                StringComparison.Ordinal)) == true;
            return Blocked(
                canonicalPath,
                containsOpenForge
                    ? "field 'open-forge' is malformed and cannot be preserved safely."
                    : "field 'frontmatter' contains metadata that cannot be preserved safely.");
        }

        var root = syntax.Root;
        var documentContext = new WorkspaceAdoptionSkillDocumentContext(
            canonicalPath,
            source,
            markdown,
            authored,
            originalBytes);
        var frontmatterContext = new WorkspaceAdoptionSkillFrontmatterContext(
            yaml,
            yamlSpan.Start,
            root,
            openForge);
        if (root?.Mapping is { } mapping)
        {
            if (FindDuplicateField(mapping, NameField, DescriptionField, TitleField, OpenForgeField) is { } duplicate)
            {
                return Blocked(canonicalPath, $"field '{duplicate}' is duplicated.");
            }

            if (mapping.Count == 0)
            {
                return PlanEmptyMapping(documentContext, frontmatterContext);
            }

            if (authored.State == SourceAuthoredMetadataState.Malformed)
            {
                return Blocked(canonicalPath, ResolveMalformedField(mapping));
            }

            return PlanMapping(documentContext, frontmatterContext);
        }

        if (root is null)
        {
            return PlanEmptyYaml(documentContext, frontmatterContext);
        }

        if (root.Scalar is { } nullScalar
            && authored.State == SourceAuthoredMetadataState.Missing
            && IsNullScalar(nullScalar)
            && IsPlainNullToken(yaml, root))
        {
            return PlanNullRoot(documentContext, frontmatterContext);
        }

        return Blocked(canonicalPath, "field 'frontmatter' is not an editable mapping.");
    }

    private WorkspaceAdoptionDocumentPlan AddMissingFrontmatter(
        string canonicalPath,
        string source,
        MarkdownDocumentFacts markdown)
    {
        var directoryName = ReadDirectoryName(canonicalPath);
        var description = ReadBodyDescription(canonicalPath, markdown, out var descriptionSource);
        var newline = WorkspaceAdoptionDocumentText.ReadPreferredNewline(source);
        var frontmatter = string.Concat(
            "---", newline,
            NameField, ": ", WorkspaceAdoptionDocumentText.QuoteYamlScalar(directoryName), newline,
            DescriptionField, ": ", WorkspaceAdoptionDocumentText.QuoteYamlScalar(description), newline,
            "---", newline,
            newline);
        var insertionPoint = source.StartsWith('\uFEFF') ? 1 : 0;
        var updated = source.Insert(insertionPoint, frontmatter);
        return CompletePlan(
            canonicalPath,
            updated,
            [NameField, DescriptionField],
            [
                new WorkspaceAdoptionDerivation(NameField, WorkspaceAdoptionDerivationSource.DirectoryName),
                new WorkspaceAdoptionDerivation(DescriptionField, descriptionSource),
            ]);
    }

    private WorkspaceAdoptionDocumentPlan PlanMapping(
        WorkspaceAdoptionSkillDocumentContext document,
        WorkspaceAdoptionSkillFrontmatterContext frontmatter)
    {
        var canonicalPath = document.CanonicalPath;
        var source = document.Source;
        var authored = document.Authored;
        var originalBytes = document.OriginalBytes;
        var yaml = frontmatter.Yaml;
        var yamlStart = frontmatter.YamlStart;
        var openForge = frontmatter.OpenForge;
        var root = frontmatter.Root
            ?? throw new InvalidOperationException("A mapping edit context requires a YAML root.");
        var mapping = root.Mapping
            ?? throw new InvalidOperationException("A mapping edit context requires a YAML mapping.");
        var edits = new List<WorkspaceAdoptionTextEdit>();
        if (root.ContainsUnsupportedMapping)
        {
            return Blocked(canonicalPath, "field 'frontmatter' contains a mapping that cannot be edited safely.");
        }

        var nameNode = ReadValue(mapping, NameField);
        var descriptionNode = ReadValue(mapping, DescriptionField);
        if (nameNode is { Kind: not (YamlNodeKind.Scalar or YamlNodeKind.Alias) })
        {
            return Blocked(canonicalPath, "field 'name' must be a scalar.");
        }

        if (descriptionNode is { Kind: not (YamlNodeKind.Scalar or YamlNodeKind.Alias) })
        {
            return Blocked(canonicalPath, "field 'description' must be a scalar.");
        }

        var nameIsMissing = nameNode is null;
        if (nameNode is { Kind: YamlNodeKind.Scalar, Scalar: { } nameScalar })
        {
            nameIsMissing = IsNullScalar(nameScalar) || string.IsNullOrWhiteSpace(nameScalar.Value);
        }

        var descriptionIsMissing = descriptionNode is null || authored.ObservedDescription is null;
        if (descriptionNode is { Kind: YamlNodeKind.Alias } && descriptionIsMissing)
        {
            return Blocked(canonicalPath, "field 'description' is an alias whose required value cannot be edited safely.");
        }

        if (!nameIsMissing && !descriptionIsMissing)
        {
            return ValidateNoEdit(canonicalPath, source, originalBytes);
        }

        var pending = new List<WorkspaceAdoptionPendingField>();
        var fields = new List<string>();
        var derivation = new List<WorkspaceAdoptionDerivation>();
        if (nameIsMissing)
        {
            if (nameNode is { Kind: YamlNodeKind.Alias }
                && authored.ObservedDescription is not null)
            {
                return Blocked(canonicalPath, "field 'name' is an empty alias and cannot be patched without flattening it.");
            }

            var name = ReadDirectoryName(canonicalPath);
            fields.Add(NameField);
            derivation.Add(new WorkspaceAdoptionDerivation(
                NameField,
                WorkspaceAdoptionDerivationSource.DirectoryName));
            if (nameNode is null)
            {
                pending.Add(new WorkspaceAdoptionPendingField(NameField, name));
            }
            else
            {
                var nameEdit = TryAddScalarEdit(
                    new WorkspaceAdoptionScalarEditRequest(frontmatter, nameNode, NameField, name),
                    out var nameCause);
                if (nameEdit is null)
                {
                    return Blocked(canonicalPath, nameCause);
                }

                edits.Add(nameEdit);
            }
        }

        if (descriptionIsMissing)
        {
            var description = ReadFrontmatterDescription(
                document,
                frontmatter,
                out var descriptionSource,
                out var descriptionCause);
            if (descriptionCause is not null)
            {
                return Blocked(canonicalPath, descriptionCause);
            }

            fields.Add(DescriptionField);
            derivation.Add(new WorkspaceAdoptionDerivation(DescriptionField, descriptionSource));
            if (descriptionNode is null)
            {
                pending.Add(new WorkspaceAdoptionPendingField(DescriptionField, description));
            }
            else
            {
                var descriptionEdit = TryAddScalarEdit(
                    new WorkspaceAdoptionScalarEditRequest(frontmatter, descriptionNode, DescriptionField, description),
                    out var descriptionEditCause);
                if (descriptionEdit is null)
                {
                    return Blocked(canonicalPath, descriptionEditCause);
                }

                edits.Add(descriptionEdit);
            }
        }

        if (pending.Count != 0)
        {
            if (root.Span.Length > 0
                && yaml.AsSpan(root.Span.Start, root.Span.Length).TrimStart().StartsWith('{'))
            {
                return Blocked(canonicalPath, "field 'frontmatter' has no safe insertion boundary in a flow mapping.");
            }

            if (!WorkspaceAdoptionDocumentText.TryReadBlockInsertionPoint(
                    yaml,
                    root.Span.Start,
                    root.Span.End,
                    out var yamlInsertion,
                    out var newline,
                    out var indentation))
            {
                return Blocked(canonicalPath, "field 'frontmatter' has no safe additive insertion boundary.");
            }

            var fieldLines = pending
                .Select(item => $"{item.Field}: {WorkspaceAdoptionDocumentText.QuoteYamlScalar(item.Value)}")
                .Select(line => indentation + line);
            edits.Add(new WorkspaceAdoptionTextEdit(
                yamlStart + yamlInsertion,
                0,
                string.Join(newline, fieldLines) + newline));
        }

        if (edits.Count == 0)
        {
            return ValidateNoEdit(canonicalPath, source, originalBytes);
        }

        if (!WorkspaceAdoptionDocumentText.TryApplyEdits(source, edits, out var updatedSource))
        {
            return Blocked(canonicalPath, "field 'frontmatter' contains overlapping edit boundaries.");
        }

        return CompletePlan(canonicalPath, updatedSource, fields, derivation);
    }

    private WorkspaceAdoptionDocumentPlan PlanEmptyYaml(
        WorkspaceAdoptionSkillDocumentContext document,
        WorkspaceAdoptionSkillFrontmatterContext frontmatter)
    {
        var canonicalPath = document.CanonicalPath;
        var source = document.Source;
        var yaml = frontmatter.Yaml;
        var markdown = document.Markdown;
        if (!yaml.AsSpan().IsWhiteSpace())
        {
            return Blocked(canonicalPath, "field 'frontmatter' is an ambiguous empty YAML document.");
        }

        var directoryName = ReadDirectoryName(canonicalPath);
        var description = ReadBodyDescription(canonicalPath, markdown, out var descriptionSource);
        var newline = WorkspaceAdoptionDocumentText.ReadPreferredNewline(source);
        var lines = CreateFieldLines(directoryName, description);
        var insertion = string.Join(newline, lines) + newline;
        var updated = source.Insert(frontmatter.YamlStart, insertion);
        return CompletePlan(
            canonicalPath,
            updated,
            [NameField, DescriptionField],
            [
                new WorkspaceAdoptionDerivation(NameField, WorkspaceAdoptionDerivationSource.DirectoryName),
                new WorkspaceAdoptionDerivation(DescriptionField, descriptionSource),
            ]);
    }

    private WorkspaceAdoptionDocumentPlan PlanEmptyMapping(
        WorkspaceAdoptionSkillDocumentContext document,
        WorkspaceAdoptionSkillFrontmatterContext frontmatter)
    {
        var canonicalPath = document.CanonicalPath;
        var source = document.Source;
        var yaml = frontmatter.Yaml;
        var markdown = document.Markdown;
        var root = frontmatter.Root
            ?? throw new InvalidOperationException("An empty mapping context requires a YAML root.");
        if (!WorkspaceAdoptionDocumentText.IsSimpleEmptyFlowMapping(
                yaml.AsSpan(root.Span.Start, root.Span.Length))
            || !WorkspaceAdoptionDocumentText.TryReadStandaloneNodeIndentation(
                yaml,
                root.Span.Start,
                root.Span.End,
                out var indentation))
        {
            return Blocked(canonicalPath, "field 'frontmatter' empty mapping has no unambiguous additive boundary.");
        }

        var directoryName = ReadDirectoryName(canonicalPath);
        var description = ReadBodyDescription(canonicalPath, markdown, out var descriptionSource);
        var newline = WorkspaceAdoptionDocumentText.ReadPreferredNewline(source);
        var replacement = string.Join(
            newline,
            CreateFieldLines(directoryName, description).Select((line, index) =>
                index == 0 ? line : indentation + line));
        var updated = source.Remove(frontmatter.YamlStart + root.Span.Start, root.Span.Length)
            .Insert(frontmatter.YamlStart + root.Span.Start, replacement);
        return CompletePlan(
            canonicalPath,
            updated,
            [NameField, DescriptionField],
            [
                new WorkspaceAdoptionDerivation(NameField, WorkspaceAdoptionDerivationSource.DirectoryName),
                new WorkspaceAdoptionDerivation(DescriptionField, descriptionSource),
            ]);
    }

    private WorkspaceAdoptionDocumentPlan PlanNullRoot(
        WorkspaceAdoptionSkillDocumentContext document,
        WorkspaceAdoptionSkillFrontmatterContext frontmatter)
    {
        var canonicalPath = document.CanonicalPath;
        var source = document.Source;
        var yaml = frontmatter.Yaml;
        var markdown = document.Markdown;
        var root = frontmatter.Root
            ?? throw new InvalidOperationException("A null root context requires a YAML root.");
        if (!WorkspaceAdoptionDocumentText.TryReadStandaloneNodeIndentation(
                yaml,
                root.Span.Start,
                root.Span.End,
                out var indentation))
        {
            return Blocked(canonicalPath, "field 'frontmatter' null value has no unambiguous additive boundary.");
        }

        var directoryName = ReadDirectoryName(canonicalPath);
        var description = ReadBodyDescription(canonicalPath, markdown, out var descriptionSource);
        var newline = WorkspaceAdoptionDocumentText.ReadPreferredNewline(source);
        var lines = CreateFieldLines(directoryName, description);
        var replacement = FormatReplacementLines(lines, indentation, newline, firstLineAlreadyIndented: true);
        var updated = source.Remove(frontmatter.YamlStart + root.Span.Start, root.Span.Length)
            .Insert(frontmatter.YamlStart + root.Span.Start, replacement);
        return CompletePlan(
            canonicalPath,
            updated,
            [NameField, DescriptionField],
            [
                new WorkspaceAdoptionDerivation(NameField, WorkspaceAdoptionDerivationSource.DirectoryName),
                new WorkspaceAdoptionDerivation(DescriptionField, descriptionSource),
            ]);
    }

    private string ReadFrontmatterDescription(
        WorkspaceAdoptionSkillDocumentContext document,
        WorkspaceAdoptionSkillFrontmatterContext frontmatter,
        out WorkspaceAdoptionDerivationSource source,
        out string? cause)
    {
        var canonicalPath = document.CanonicalPath;
        var markdown = document.Markdown;
        var mapping = frontmatter.Root?.Mapping ?? [];
        var openForge = frontmatter.OpenForge;
        source = WorkspaceAdoptionDerivationSource.RelativePath;
        cause = null;

        if (!string.IsNullOrWhiteSpace(openForge.ObservedDescription))
        {
            source = WorkspaceAdoptionDerivationSource.ExistingDescription;
            return openForge.ObservedDescription;
        }

        var title = ReadValue(mapping, TitleField);
        if (title is { Kind: not YamlNodeKind.Alias })
        {
            if (title.Scalar is null)
            {
                cause = "field 'title' must be a scalar to derive a description.";
                return string.Empty;
            }

            if (!IsNullScalar(title.Scalar) && !string.IsNullOrWhiteSpace(title.Scalar.Value))
            {
                source = WorkspaceAdoptionDerivationSource.ExistingTitle;
                return title.Scalar.Value;
            }
        }

        return ReadBodyDescription(canonicalPath, markdown, out source);
    }

    private WorkspaceAdoptionDocumentPlan ValidateNoEdit(
        string canonicalPath,
        string source,
        ReadOnlyMemory<byte> originalBytes)
    {
        var markdown = _markdownParser.Parse(source);
        var authored = _authoredMetadataParser.Parse(markdown, SourceDocumentForm.Skill);
        if (authored.ApplyTo.State == ApplyToMetadataState.Invalid)
        {
            return Blocked(canonicalPath, "field 'applyTo' is invalid.");
        }

        if (authored.State != SourceAuthoredMetadataState.Complete)
        {
            return Blocked(canonicalPath, "fields 'name' and 'description' cannot be completed safely.");
        }

        return Unchanged(originalBytes);
    }

    private WorkspaceAdoptionDocumentPlan CompletePlan(
        string canonicalPath,
        string updatedSource,
        IEnumerable<string> fields,
        IEnumerable<WorkspaceAdoptionDerivation> derivation)
    {
        var markdown = _markdownParser.Parse(updatedSource);
        var authored = _authoredMetadataParser.Parse(markdown, SourceDocumentForm.Skill);
        if (authored.ApplyTo.State == ApplyToMetadataState.Invalid)
        {
            return Blocked(canonicalPath, "field 'applyTo' became invalid after the metadata edit.");
        }

        if (authored.State != SourceAuthoredMetadataState.Complete)
        {
            return Blocked(canonicalPath, "fields 'name' and 'description' cannot be completed without changing authored aliases.");
        }

        if (!WorkspaceAdoptionDocumentText.TryEncode(updatedSource, out var intendedBytes))
        {
            return Blocked(canonicalPath, "field 'frontmatter' cannot be represented as strict UTF-8.");
        }

        return new WorkspaceAdoptionDocumentPlan(
            intendedBytes,
            [WorkspaceAdoptionAction.MetadataCompleted],
            fields,
            derivation,
            cause: null);
    }

    private static WorkspaceAdoptionTextEdit? TryAddScalarEdit(
        WorkspaceAdoptionScalarEditRequest request,
        out string cause)
    {
        var yaml = request.Frontmatter.Yaml;
        var node = request.Node;
        var field = request.Field;
        var value = request.Value;
        cause = string.Empty;
        if (node.Scalar is not { } scalar)
        {
            cause = $"field '{field}' is not a scalar and cannot be patched safely.";
            return null;
        }

        var nodeText = yaml.AsSpan(node.Span.Start, node.Span.Length).TrimStart();
        if (nodeText.StartsWith('&') || nodeText.StartsWith('!'))
        {
            cause = $"field '{field}' has a YAML anchor or tag that cannot be preserved by a scalar patch.";
            return null;
        }

        var prefix = scalar.Span.Length == 0 ? " " : string.Empty;
        return new WorkspaceAdoptionTextEdit(
            request.Frontmatter.YamlStart + scalar.Span.Start,
            scalar.Span.Length,
            prefix + WorkspaceAdoptionDocumentText.QuoteYamlScalar(value));
    }

    private static string? FindDuplicateField(
        IReadOnlyList<YamlMappingEntry> mapping,
        params string[] fields)
    {
        foreach (var field in fields)
        {
            if (mapping.Count(entry => string.Equals(entry.Key.Scalar?.Value, field, StringComparison.Ordinal)) > 1)
            {
                return field;
            }
        }

        return null;
    }

    private static string ResolveMalformedField(IReadOnlyList<YamlMappingEntry> mapping)
    {
        foreach (var field in new[] { NameField, DescriptionField })
        {
            var value = ReadValue(mapping, field);
            if (value is { Kind: YamlNodeKind.Sequence or YamlNodeKind.Mapping })
            {
                return $"field '{field}' must be a scalar.";
            }
        }

        return "field 'frontmatter' contains an unsupported native metadata value.";
    }

    private static YamlNode? ReadValue(IReadOnlyList<YamlMappingEntry> mapping, string field)
        => mapping
            .Where(entry => string.Equals(entry.Key.Scalar?.Value, field, StringComparison.Ordinal))
            .Select(entry => entry.Value)
            .FirstOrDefault();

    private static bool IsNullScalar(YamlScalar scalar)
    {
        if (scalar.Tag is "tag:yaml.org,2002:null" or "!!null" or "!<tag:yaml.org,2002:null>")
        {
            return true;
        }

        return scalar.Style == ScalarStyle.Plain
            && scalar.IsPlainImplicit
            && !scalar.IsQuotedImplicit
            && (scalar.Value == "~" || string.Equals(scalar.Value, "null", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPlainNullToken(string yaml, YamlNode root)
    {
        var token = yaml.AsSpan(root.Span.Start, root.Span.Length).Trim();
        return token.SequenceEqual("~")
            || token.Equals("null", StringComparison.OrdinalIgnoreCase)
            || token.Equals("NULL", StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadBodyDescription(
        string canonicalPath,
        MarkdownDocumentFacts markdown,
        out WorkspaceAdoptionDerivationSource source)
    {
        var heading = markdown.Headings.FirstOrDefault(candidate =>
            candidate.IsTopLevel
            && candidate.Level == 1
            && !string.IsNullOrWhiteSpace(candidate.VisibleText));
        if (heading is not null)
        {
            source = WorkspaceAdoptionDerivationSource.Heading;
            return heading.VisibleText;
        }

        source = WorkspaceAdoptionDerivationSource.RelativePath;
        return canonicalPath;
    }

    private static string ReadDirectoryName(string canonicalPath)
    {
        var parent = SourceLogicalPath.ReadParent(canonicalPath);
        var separator = parent.LastIndexOf('/');
        return parent[(separator + 1)..];
    }

    private static IReadOnlyList<string> CreateFieldLines(string name, string description)
        => [
            $"{NameField}: {WorkspaceAdoptionDocumentText.QuoteYamlScalar(name)}",
            $"{DescriptionField}: {WorkspaceAdoptionDocumentText.QuoteYamlScalar(description)}",
        ];

    private static string FormatReplacementLines(
        IReadOnlyList<string> lines,
        string indentation,
        string newline,
        bool firstLineAlreadyIndented)
    {
        var formatted = new string[lines.Count];
        for (var index = 0; index < lines.Count; index++)
        {
            formatted[index] = index == 0 && firstLineAlreadyIndented
                ? lines[index]
                : indentation + lines[index];
        }

        return string.Join(newline, formatted);
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
