using System.Text;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Documents;

internal sealed class FindFrontmatterReader
{
    private readonly IDeserializer _skillDeserializer = new StaticDeserializerBuilder(
            new FrameworkMetadataYamlContext())
        .IgnoreUnmatchedProperties()
        .Build();
    private readonly FrameworkDocumentMetadataParser _metadataParser = new();
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
            if (input.Layer.Form == SourceDocumentForm.Skill)
            {
                return ReadSkill(yaml);
            }

            var metadataFacts = _metadataParser.Parse(input.Document);
            if (metadataFacts.State == FrameworkDocumentMetadataState.Malformed)
            {
                return Unavailable();
            }

            if (metadataFacts.State == FrameworkDocumentMetadataState.Missing)
            {
                return Complete(null, []);
            }

            var metadata = metadataFacts.Metadata
                ?? throw new InvalidOperationException(
                    "Complete Framework document metadata facts require authored values.");
            var tags = new List<FindFrontmatterTagOccurrence>(metadata.Tags.Length);
            for (var index = 0; index < metadata.Tags.Length; index++)
            {
                var span = metadataFacts.TagSpans[index];
                var start = checked(yamlSpan.Start + span.Start);
                tags.Add(new FindFrontmatterTagOccurrence(
                    metadata.Tags[index],
                    MapLocation(input.Document.Source, start, span.Length)));
            }

            return Complete(metadata.Description, tags);
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

    private FindFrontmatterFacts ReadSkill(string yaml)
    {
        var syntax = _yamlParser.Parse(yaml);
        if (syntax.State != YamlDocumentState.Complete
            || syntax.HasAliases
            || syntax.HasUnsupportedMappings
            || syntax.Root is not null && syntax.Root.Kind != YamlNodeKind.Mapping)
        {
            return Unavailable();
        }

        var skill = _skillDeserializer.Deserialize<FrameworkSkillMetadataYamlDocument>(yaml);
        return Complete(skill?.Description, []);
    }

    private static bool IsUnavailableYamlFailure(Exception exception)
    {
        return exception is YamlException
            or InvalidOperationException
            or ArgumentException
            or InvalidCastException
            or FormatException;
    }

    private static SourceLocation MapLocation(string source, int start, int length)
        => new Utf8SourceMap(source).Map(start, length);
}
