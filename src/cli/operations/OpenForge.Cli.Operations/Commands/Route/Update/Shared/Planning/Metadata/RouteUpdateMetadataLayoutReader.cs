using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning.Metadata;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning.Metadata;

internal sealed partial class RouteUpdateMetadataLayoutReader
{
    internal RouteUpdateMetadataLayoutRead Read(RouteUpdateObservation observation)
    {
        var yaml = observation.Frontmatter;
        var syntax = observation.Metadata.Syntax;
        var source = yaml?.Source ?? string.Empty;
        var emptyMapping = syntax.ScopedEntry is { } scopedEntry
            && scopedEntry.Value.Mapping is { Count: 0 }
            && IsEmptyFlowMapping(source, scopedEntry.Value)
                ? ReadEmptyMapping(source, scopedEntry)
                : null;
        if (observation.Markdown.Frontmatter.State == MarkdownFrontmatterState.Unavailable
            || yaml is not null && (yaml.State != YamlDocumentState.Complete || yaml.HasAliases)
            || yaml?.Root is { Mapping: null } && !string.IsNullOrWhiteSpace(source)
            || observation.Metadata.FailureKind == FrameworkDocumentMetadataFailureKind.Duplicate
            || syntax.AuthoredForm is not null && syntax.SelectedMapping is null
            || syntax.SelectedMapping is { } selectedValue
                && IsFlowMapping(source, selectedValue) && emptyMapping is null
                    && !(syntax.AuthoredForm is null && selectedValue.Mapping is { Count: 0 }
                        && IsEmptyFlowMapping(source, selectedValue))
            || syntax.Members.GroupBy(member => member.Field).Any(group => group.Count() != 1))
        {
            return RouteUpdateMetadataLayoutRead.Unsafe(
                "The target frontmatter does not expose one safe parsed YAML mapping.");
        }

        var description = ReadMember(source, syntax, FrameworkMetadataField.Description, "description");
        var responsibility = ReadMember(source, syntax, FrameworkMetadataField.Responsibility, "responsibility");
        var tags = ReadMember(source, syntax, FrameworkMetadataField.Tags, "tags");
        var applyTo = observation.Metadata.ApplyTo;
        if (applyTo.State == ApplyToMetadataState.Invalid)
        {
            return RouteUpdateMetadataLayoutRead.Unsafe(
                "The target applyTo metadata is invalid or ambiguous.");
        }

        var applyToMembers = ImmutableArray.CreateBuilder<RouteUpdateMetadataMember>();
        foreach (var declaration in applyTo.Declarations)
        {
            var entry = ReadDeclarationEntry(yaml, syntax, declaration);
            var line = entry is null
                ? null
                : TryReadApplyToMemberLine(source, entry);
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

        var creation = ReadCreationBoundary(observation);
        return RouteUpdateMetadataLayoutRead.Complete(
            new RouteUpdateMetadataLayout
            {
                Source = source,
                Syntax = syntax,
                DocumentOffset = observation.Markdown.Frontmatter.YamlSpan?.Start ?? 0,
                CreationStart = creation.Start,
                CreationLength = creation.Length,
                CreateHeader = observation.Markdown.Frontmatter.State == MarkdownFrontmatterState.Missing,
                Newline = ReadNewline(observation.TargetText),
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

    private static (int Start, int Length) ReadCreationBoundary(RouteUpdateObservation observation)
    {
        var yamlSpan = observation.Markdown.Frontmatter.YamlSpan;
        if (observation.Metadata.Syntax.AuthoredForm is null
            && observation.Metadata.Syntax.SelectedMapping is { } root
            && observation.Frontmatter is { } yaml && IsEmptyFlowMapping(yaml.Source, root))
        {
            return ((yamlSpan?.Start ?? 0) + root.Span.Start, root.Span.Length);
        }

        if (yamlSpan is not null)
        {
            return (yamlSpan.End, 0);
        }

        var start = observation.TargetText.StartsWith("\uFEFF", StringComparison.Ordinal) ? 1 : 0;
        return (start, 0);
    }

    private static RouteUpdateMetadataMember? ReadMember(
        string source,
        FrameworkDocumentMetadataSyntax syntax,
        FrameworkMetadataField field,
        string name)
    {
        var member = syntax.Members.SingleOrDefault(value => value.Field == field);
        if (member is null)
        {
            return null;
        }

        return new RouteUpdateMetadataMember
        {
            Name = name,
            Entry = member.Entry,
            Line = TryReadMemberLine(source, name, member.Entry,
                allowTrailingComment: syntax.AuthoredForm == FrontmatterForm.Root,
                preserveRootTrivia: syntax.AuthoredForm == FrontmatterForm.Root),
        };
    }

    private static string ReadNewline(string source)
    {
        var position = source.IndexOfAny(['\r', '\n']);
        if (position < 0)
        {
            return "\n";
        }

        return source[position] == '\r' && position + 1 < source.Length && source[position + 1] == '\n'
            ? "\r\n"
            : source[position].ToString();
    }

}
