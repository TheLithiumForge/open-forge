using System.Text;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using OpenForge.Cli.Core.Shell.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Source;

internal sealed class RouteMetadataParser
{
    private readonly IDeserializer _deserializer = new StaticDeserializerBuilder(new CliYamlContext()).Build();
    private readonly MarkdownFrontmatterParser _frontmatterParser = new();
    private readonly YamlDocumentParser _yamlParser = new();

    internal RouteSourceMetadata ParseOpenForge(
        string sourceBody,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent)
    {
        ArgumentNullException.ThrowIfNull(sourceBody);
        var frontmatter = ReadFrontmatter(sourceBody);
        if (frontmatter.State != RouteSourceMetadataState.Complete
            || frontmatter.Yaml is not { } yaml)
        {
            return WithoutValues(frontmatter.State, isCompatibilityEntrypoint, isOverwritePresent);
        }

        try
        {
            var authored = _deserializer.Deserialize<CliAuthoredMetadata>(yaml);
            if (authored?.OpenForge is null
                || string.IsNullOrWhiteSpace(authored.OpenForge.Description)
                || authored.OpenForge.Tags is null
                || authored.OpenForge.Tags.Length == 0)
            {
                return RouteSourceMetadata.WithoutValues(
                    RouteSourceMetadataState.Missing,
                    isCompatibilityEntrypoint,
                    isOverwritePresent);
            }

            if (authored.OpenForge.Tags.Any(tag => !IsValidTag(tag)))
            {
                return RouteSourceMetadata.WithoutValues(
                    RouteSourceMetadataState.Malformed,
                    isCompatibilityEntrypoint,
                    isOverwritePresent);
            }

            return RouteSourceMetadata.Complete(
                authored.OpenForge.Description,
                authored.OpenForge.Tags,
                isCompatibilityEntrypoint,
                isOverwritePresent);
        }
        catch (YamlException)
        {
            return RouteSourceMetadata.WithoutValues(
                RouteSourceMetadataState.Malformed,
                isCompatibilityEntrypoint,
                isOverwritePresent);
        }
    }

    internal RouteSourceMetadata ParseSkill(string sourceBody, bool isOverwritePresent)
    {
        ArgumentNullException.ThrowIfNull(sourceBody);
        var frontmatter = ReadFrontmatter(sourceBody);
        if (frontmatter.State != RouteSourceMetadataState.Complete
            || frontmatter.Yaml is not { } yaml)
        {
            return WithoutValues(frontmatter.State, isCompatibilityEntrypoint: false, isOverwritePresent);
        }

        try
        {
            var skill = _deserializer.Deserialize<CliSkillMetadata>(yaml);
            if (skill is null
                || string.IsNullOrWhiteSpace(skill.Name)
                || string.IsNullOrWhiteSpace(skill.Description))
            {
                return RouteSourceMetadata.WithoutValues(
                    RouteSourceMetadataState.Missing,
                    isCompatibilityEntrypoint: false,
                    isOverwritePresent);
            }

            return RouteSourceMetadata.Complete(
                skill.Description,
                [],
                isCompatibilityEntrypoint: false,
                isOverwritePresent);
        }
        catch (YamlException)
        {
            return RouteSourceMetadata.WithoutValues(
                RouteSourceMetadataState.Malformed,
                isCompatibilityEntrypoint: false,
                isOverwritePresent);
        }
    }

    internal static bool IsValidTag(string? tag)
    {
        if (string.IsNullOrEmpty(tag))
        {
            return false;
        }

        var runes = tag.EnumerateRunes().ToArray();
        if (runes.Length == 0 || !Rune.IsLetter(runes[0]))
        {
            return false;
        }

        var previousWasHyphen = false;
        for (var index = 1; index < runes.Length; index++)
        {
            var rune = runes[index];
            if (rune.Value == '-')
            {
                if (previousWasHyphen || index == runes.Length - 1)
                {
                    return false;
                }

                previousWasHyphen = true;
                continue;
            }

            if (!Rune.IsLetterOrDigit(rune))
            {
                return false;
            }

            previousWasHyphen = false;
        }

        return true;
    }

    private (RouteSourceMetadataState State, string? Yaml) ReadFrontmatter(string sourceBody)
    {
        var boundary = _frontmatterParser.Parse(sourceBody);
        if (boundary.State != MarkdownFrontmatterState.Complete
            || boundary.YamlSpan is not { } yamlSpan)
        {
            return (
                boundary.State == MarkdownFrontmatterState.Missing
                    ? RouteSourceMetadataState.Missing
                    : RouteSourceMetadataState.Malformed,
                null);
        }

        var yaml = sourceBody[yamlSpan.Start..yamlSpan.End];
        var syntax = _yamlParser.Parse(yaml);
        return syntax.State == YamlDocumentState.Complete
            ? (RouteSourceMetadataState.Complete, yaml)
            : (RouteSourceMetadataState.Malformed, null);
    }

    private static RouteSourceMetadata WithoutValues(
        RouteSourceMetadataState state,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent)
    {
        if (state is not (RouteSourceMetadataState.Missing or RouteSourceMetadataState.Malformed))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "Complete metadata requires semantic parsing.");
        }

        return RouteSourceMetadata.WithoutValues(
            state,
            isCompatibilityEntrypoint,
            isOverwritePresent);
    }
}
