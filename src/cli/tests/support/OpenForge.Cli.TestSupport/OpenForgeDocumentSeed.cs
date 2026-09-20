namespace OpenForge.Cli.TestSupport;

/// <summary>
/// Builds valid, in-memory Open Forge Markdown documents from small composable
/// fixture inputs. Malformed and exact-byte protocol oracles remain local to
/// their tests.
/// </summary>
public static class OpenForgeDocumentSeed
{
    private const string EntriesHeading = "## Entries";
    private const string DefaultSkillBody = "# Skill\n\n";
    private const string SkillEntrypointTitle = "Skills";
    private const string SkillTag = "Skill";

    /// <summary>
    /// Builds a document with valid Open Forge metadata and an exact caller-owned
    /// body.
    /// </summary>
    public static string Metadata(
        string description,
        IEnumerable<string> tags,
        string body)
    {
        ArgumentNullException.ThrowIfNull(body);

        return $"{MetadataFrontmatter(description, tags)}\n{body}";
    }

    /// <summary>
    /// Builds valid Open Forge frontmatter without adding body whitespace.
    /// </summary>
    public static string MetadataFrontmatter(
        string description,
        IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(tags);

        var tagList = string.Join(", ", tags);
        return $"---\nopen-forge:\n  description: {description}\n  tags: [{tagList}]\n---";
    }

    /// <summary>
    /// Builds a Markdown document whose final Entries section contains one exact
    /// heading-owned generated body.
    /// </summary>
    public static string GeneratedEntries(string entries)
    {
        return GeneratedEntries(new GeneratedEntriesSeed
        {
            Entries = entries,
        });
    }

    /// <summary>
    /// Builds a configured Markdown document whose final Entries section contains
    /// one exact heading-owned generated body.
    /// </summary>
    public static string GeneratedEntries(GeneratedEntriesSeed seed)
    {
        ArgumentNullException.ThrowIfNull(seed);
        ArgumentNullException.ThrowIfNull(seed.Entries);
        ArgumentNullException.ThrowIfNull(seed.LineEnding);
        ArgumentNullException.ThrowIfNull(seed.Prefix);

        var finalLineEnding = seed.IncludeFinalLineEnding ? seed.LineEnding : string.Empty;
        return $"{seed.Prefix}{seed.LineEnding}{seed.LineEnding}"
            + $"{EntriesHeading}{seed.LineEnding}{seed.LineEnding}"
            + $"{seed.Entries}{finalLineEnding}";
    }

    /// <summary>
    /// Builds a valid Skill document with exact name, description, and body.
    /// </summary>
    public static string Skill(
        string name,
        string description,
        string body = DefaultSkillBody)
    {
        ArgumentNullException.ThrowIfNull(body);

        return $"{SkillFrontmatter(name, description)}\n{body}";
    }

    /// <summary>
    /// Builds the routed Skills entrypoint for a non-empty set of Skill package
    /// names.
    /// </summary>
    public static string SkillEntrypoint(IEnumerable<string> skillNames)
    {
        ArgumentNullException.ThrowIfNull(skillNames);
        var names = skillNames
            .Select(name =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(name);
                return name;
            })
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (names.Length == 0 || names.Distinct(StringComparer.Ordinal).Count() != names.Length)
        {
            throw new ArgumentException(
                "Routed Skill fixture names must be non-empty and unique.",
                nameof(skillNames));
        }

        var entries = string.Join(
            '\n',
            names.Select(name => $"- [{name}]({name}/SKILL.md) - #{SkillTag}"));
        return Metadata(
            description: SkillEntrypointTitle,
            tags: [SkillTag],
            body: $"\n{GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = entries,
                Prefix = $"# {SkillEntrypointTitle}",
            })}");
    }

    /// <summary>
    /// Builds valid Skill frontmatter without adding body whitespace.
    /// </summary>
    public static string SkillFrontmatter(string name, string description)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(description);

        return $"---\nname: {name}\ndescription: {description}\n---";
    }
}

/// <summary>
/// Describes one valid generated Entries document without an error-prone group
/// of same-type positional parameters.
/// </summary>
public sealed record GeneratedEntriesSeed
{
    /// <summary>
    /// Gets the exact content in the generated Entries body.
    /// </summary>
    public required string Entries { get; init; }

    /// <summary>
    /// Gets the exact Markdown content before the blank line and Entries heading.
    /// </summary>
    public string Prefix { get; init; } = "# Root";

    /// <summary>
    /// Gets the line ending used for generated document structure.
    /// </summary>
    public string LineEnding { get; init; } = "\n";

    /// <summary>
    /// Gets whether the generated document ends with its selected line ending.
    /// </summary>
    public bool IncludeFinalLineEnding { get; init; } = true;
}
