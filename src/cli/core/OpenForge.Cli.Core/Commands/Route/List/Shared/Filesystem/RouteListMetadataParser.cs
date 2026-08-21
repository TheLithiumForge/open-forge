using System.Text;
using OpenForge.Cli.Core.Shell.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed class RouteListMetadataParser
{
    private enum FrontmatterState
    {
        Complete,
        Missing,
        Malformed,
    }

    private readonly IDeserializer _deserializer = new StaticDeserializerBuilder(new CliYamlContext()).Build();

    internal RouteListSourceMetadata ParseOpenForge(
        string sourceBody,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent)
    {
        ArgumentNullException.ThrowIfNull(sourceBody);
        var frontmatter = ExtractFrontmatter(sourceBody);
        if (frontmatter.State != FrontmatterState.Complete)
        {
            return WithoutValues(frontmatter.State, isCompatibilityEntrypoint, isOverwritePresent);
        }

        try
        {
            var authored = _deserializer.Deserialize<CliAuthoredMetadata>(frontmatter.Yaml!);
            if (authored?.OpenForge is null
                || string.IsNullOrWhiteSpace(authored.OpenForge.Description)
                || authored.OpenForge.Tags is null
                || authored.OpenForge.Tags.Length == 0)
            {
                return RouteListSourceMetadata.WithoutValues(
                    RouteListMetadataState.Missing,
                    isCompatibilityEntrypoint,
                    isOverwritePresent);
            }

            if (authored.OpenForge.Tags.Any(tag => !IsValidTag(tag)))
            {
                return RouteListSourceMetadata.WithoutValues(
                    RouteListMetadataState.Malformed,
                    isCompatibilityEntrypoint,
                    isOverwritePresent);
            }

            return RouteListSourceMetadata.Complete(
                authored.OpenForge.Description,
                authored.OpenForge.Tags,
                isCompatibilityEntrypoint,
                isOverwritePresent);
        }
        catch (YamlException)
        {
            return RouteListSourceMetadata.WithoutValues(
                RouteListMetadataState.Malformed,
                isCompatibilityEntrypoint,
                isOverwritePresent);
        }
    }

    internal RouteListSourceMetadata ParseSkill(string sourceBody, bool isOverwritePresent)
    {
        ArgumentNullException.ThrowIfNull(sourceBody);
        var frontmatter = ExtractFrontmatter(sourceBody);
        if (frontmatter.State != FrontmatterState.Complete)
        {
            return WithoutValues(frontmatter.State, isCompatibilityEntrypoint: false, isOverwritePresent);
        }

        try
        {
            var skill = _deserializer.Deserialize<CliSkillMetadata>(frontmatter.Yaml!);
            if (skill is null
                || string.IsNullOrWhiteSpace(skill.Name)
                || string.IsNullOrWhiteSpace(skill.Description))
            {
                return RouteListSourceMetadata.WithoutValues(
                    RouteListMetadataState.Missing,
                    isCompatibilityEntrypoint: false,
                    isOverwritePresent);
            }

            return RouteListSourceMetadata.Complete(
                skill.Description,
                [],
                isCompatibilityEntrypoint: false,
                isOverwritePresent);
        }
        catch (YamlException)
        {
            return RouteListSourceMetadata.WithoutValues(
                RouteListMetadataState.Malformed,
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

    private static (FrontmatterState State, string? Yaml) ExtractFrontmatter(string sourceBody)
    {
        using var reader = new StringReader(sourceBody);
        if (!string.Equals(reader.ReadLine(), "---", StringComparison.Ordinal))
        {
            return (FrontmatterState.Missing, null);
        }

        var yaml = new StringBuilder();
        while (reader.ReadLine() is { } line)
        {
            if (string.Equals(line, "---", StringComparison.Ordinal))
            {
                return (FrontmatterState.Complete, yaml.ToString());
            }

            yaml.AppendLine(line);
        }

        return (FrontmatterState.Malformed, null);
    }

    private static RouteListSourceMetadata WithoutValues(
        FrontmatterState state,
        bool isCompatibilityEntrypoint,
        bool isOverwritePresent)
    {
        var metadataState = state switch
        {
            FrontmatterState.Missing => RouteListMetadataState.Missing,
            FrontmatterState.Malformed => RouteListMetadataState.Malformed,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Complete frontmatter requires metadata parsing."),
        };
        return RouteListSourceMetadata.WithoutValues(
            metadataState,
            isCompatibilityEntrypoint,
            isOverwritePresent);
    }
}
