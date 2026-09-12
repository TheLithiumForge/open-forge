using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedContextWorkspace : IDisposable
{
    internal const string ExperienceDesignSkillPath = ".agents/skills/experience-design/SKILL.md";
    internal const string AccessibilitySkillPath = ".agents/skills/accessibility/SKILL.md";

    private readonly TemporaryWorkspace _workspace;

    private PublishedContextWorkspace(TemporaryWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal string Path => _workspace.Path;

    internal string Combine(string relativePath) => _workspace.Combine(relativePath);

    internal void WriteText(string relativePath, string content) => _workspace.WriteText(relativePath, content);

    internal void ReplaceText(string relativePath, string content) => _workspace.ReplaceText(relativePath, content);

    internal void AddRoutedSkills(IReadOnlyDictionary<string, string> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);
        if (documents.Count == 0)
        {
            throw new ArgumentException("At least one routed Skill document is required.", nameof(documents));
        }

        _workspace.ReplaceText(
            ".agents/loader.md",
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = "- [Startup](startup/_startup.md) - #LoadNow #Core\n"
                    + "- [Projects](projects/_projects.md) - #Project\n"
                    + "- [Skills](skills/_skills.md) - #Skill",
                Prefix = "# Loader",
            }));
        _workspace.WriteText(
            ".agents/skills/_skills.md",
            OpenForgeDocumentSeed.SkillEntrypoint(documents.Keys));
        foreach (var document in documents)
        {
            _workspace.WriteText($".agents/skills/{document.Key}/SKILL.md", document.Value);
        }
    }

    internal IReadOnlyDictionary<string, string> SnapshotState() => _workspace.SnapshotHashes();

    internal static PublishedContextWorkspace Create()
    {
        var workspace = TemporaryWorkspace.Create("e2e-context");
        try
        {
            workspace.WriteText("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
            workspace.WriteText(
                ".agents/loader.md",
                "# Loader\n\n## Entries\n\n"
                + "<!-- open-forge:generated-index:start -->\n"
                + "- [Startup](startup/_startup.md) - #LoadNow #Core\n"
                + "- [Projects](projects/_projects.md) - #Project\n"
                + "<!-- open-forge:generated-index:end -->\n");
            workspace.WriteText(
                ".agents/startup/_startup.md",
                Document(
                    "Startup",
                    "LoadNow, Core",
                    "# Startup\n\n## Entries\n\n"
                    + "<!-- open-forge:generated-index:start -->\n"
                    + "- [Topic](topic.md) - #KeepInMind #Core\n"
                    + "<!-- open-forge:generated-index:end -->\n"));
            workspace.WriteText(
                ".agents/startup/topic.md",
                Document("Topic", "KeepInMind, Core", "# Topic\n\nStartup topic.\n"));
            workspace.WriteText(
                ".agents/projects/_projects.md",
                Document(
                    "Projects",
                    "Project",
                    "# Projects\n\n## Entries\n\n"
                    + "<!-- open-forge:generated-index:start -->\n"
                    + "- [Guide](guide.md) - #KeepInMind #Guide\n"
                    + "- [Linked](linked.md) - #Guide\n"
                    + "- [Broken](broken.md) - #Guide\n"
                    + "<!-- open-forge:generated-index:end -->\n"));
            workspace.WriteText(
                ".agents/projects/guide.md",
                Document(
                    "Guide",
                    "KeepInMind, Guide",
                    "# Guide\n\n## Rules\n\nBase rule.\n\n"
                    + "[Linked](linked.md#details) [Outside](../../README.md) "
                    + "[External](https://example.invalid/context).\n"));
            workspace.WriteText(
                ".agents/projects/guide.overwrite.md",
                Document("Guide overwrite", "Guide", "# Guide overwrite\n\n## Rules\n\nOverride.\n"));
            workspace.WriteText(
                ".agents/projects/linked.md",
                Document("Linked", "Guide", "# Linked\n\n## Details\n\n[Back](guide.md).\n"));
            workspace.WriteText(
                ".agents/projects/broken.md",
                Document("Broken", "Guide", "# Broken\n\n[Missing](missing.md).\n"));
            workspace.WriteText("README.md", "# Readme\n\nContained outside agents.\n");
            return new PublishedContextWorkspace(workspace);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static string Document(string description, string tags, string body)
        => OpenForgeDocumentSeed.Metadata(
            description: description,
            tags: tags.Split(", ", StringSplitOptions.None),
            body: $"\n{body}");

    public void Dispose() => _workspace.Dispose();
}
