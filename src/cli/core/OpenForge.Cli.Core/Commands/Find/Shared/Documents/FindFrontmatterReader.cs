using System.Text;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Tags;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Shell.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Documents;

internal sealed class FindFrontmatterReader
{
    private readonly IDeserializer _deserializer = new StaticDeserializerBuilder(new CliYamlContext())
        .IgnoreUnmatchedProperties()
        .Build();
    private readonly YamlDocumentParser _yamlParser = new();

    internal FindFrontmatterFacts Read(FindFrontmatterInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var boundary = input.Document.Frontmatter;
        if (boundary.State == MarkdownFrontmatterState.Missing)
        {
            return Complete(null, []);
        }

        if (boundary.State != MarkdownFrontmatterState.Complete
            || boundary.YamlSpan is not { } yamlSpan)
        {
            return Unavailable();
        }

        var yaml = input.Document.Source[yamlSpan.Start..yamlSpan.End];
        try
        {
            var syntax = _yamlParser.Parse(yaml);
            if (syntax.State != YamlDocumentState.Complete
                || syntax.HasAliases
                || syntax.HasUnsupportedMappings)
            {
                return Unavailable();
            }

            if (syntax.Root is not null && syntax.Root.Kind != YamlNodeKind.Mapping)
            {
                return Unavailable();
            }

            if (input.Layer.Form == SourceDocumentForm.Skill)
            {
                var skill = _deserializer.Deserialize<CliSkillMetadata>(yaml);
                return Complete(skill?.Description, []);
            }

            var authored = _deserializer.Deserialize<CliAuthoredMetadata>(yaml);
            if (!TryReadOpenForgeTags(
                    syntax.Root,
                    out var tagScalars,
                    out var tagsSpecified,
                    out var openForgeSpecified))
            {
                return Unavailable();
            }

            if (authored?.OpenForge is null)
            {
                return openForgeSpecified ? Unavailable() : Complete(null, []);
            }

            var semanticTags = authored.OpenForge.Tags ?? [];
            if (!tagsSpecified && semanticTags.Length != 0
                || tagsSpecified && semanticTags.Length != tagScalars.Count)
            {
                return Unavailable();
            }

            var tags = new List<FindFrontmatterTagOccurrence>(semanticTags.Length);
            for (var index = 0; index < semanticTags.Length; index++)
            {
                var scalar = tagScalars[index];
                if (!string.Equals(semanticTags[index], scalar.Value, StringComparison.Ordinal))
                {
                    return Unavailable();
                }

                if (!FindTagGrammar.IsValid(semanticTags[index]))
                {
                    continue;
                }

                var start = checked(yamlSpan.Start + scalar.Span.Start);
                tags.Add(new FindFrontmatterTagOccurrence(
                    semanticTags[index],
                    MapLocation(input.Document.Source, start, scalar.Span.Length)));
            }

            return Complete(authored.OpenForge.Description, tags);
        }
        catch (Exception exception) when (IsUnavailableYamlFailure(exception))
        {
            return Unavailable();
        }
    }

    private static FindFrontmatterFacts Complete(
        string? description,
        IEnumerable<FindFrontmatterTagOccurrence> tags)
        => new(FindFrontmatterAvailability.Complete, description, tags);

    private static FindFrontmatterFacts Unavailable()
        => new(FindFrontmatterAvailability.Unavailable, null, []);

    private static bool IsUnavailableYamlFailure(Exception exception)
    {
        return exception is YamlException
            or InvalidOperationException
            or ArgumentException
            or InvalidCastException
            or FormatException;
    }

    private static bool TryReadOpenForgeTags(
        YamlNode? root,
        out IReadOnlyList<YamlScalar> tagScalars,
        out bool tagsSpecified,
        out bool openForgeSpecified)
    {
        tagScalars = [];
        tagsSpecified = false;
        openForgeSpecified = false;
        if (root is null || !root.TryGetMappingValue("open-forge", out var openForge))
        {
            return true;
        }

        openForgeSpecified = true;
        if (openForge?.Kind != YamlNodeKind.Mapping)
        {
            return false;
        }

        if (!openForge.TryGetMappingValue("tags", out var tags))
        {
            return true;
        }

        tagsSpecified = true;
        if (tags?.Sequence is not { } items)
        {
            return false;
        }

        var scalars = new List<YamlScalar>(items.Count);
        foreach (var item in items)
        {
            if (item.Scalar is not { } scalar)
            {
                return false;
            }

            scalars.Add(scalar);
        }

        tagScalars = scalars;
        return true;
    }

    private static SourceLocation MapLocation(string source, int start, int length)
        => new Utf8SourceMap(source).Map(start, length);
}
