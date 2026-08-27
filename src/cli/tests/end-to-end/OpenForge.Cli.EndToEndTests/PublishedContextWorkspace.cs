using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedContextWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _workspace;

    private PublishedContextWorkspace(TemporaryWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal string Path => _workspace.Path;

    internal string Combine(string relativePath) => _workspace.Combine(relativePath);

    internal void WriteText(string relativePath, string content) => _workspace.WriteText(relativePath, content);

    internal void ReplaceText(string relativePath, string content) => File.WriteAllText(_workspace.Combine(relativePath), content);

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
                    + "- [Topic](topic.md) - #LoadNow #Core\n"
                    + "<!-- open-forge:generated-index:end -->\n"));
            workspace.WriteText(
                ".agents/startup/topic.md",
                Document("Topic", "LoadNow, Core", "# Topic\n\nStartup topic.\n"));
            workspace.WriteText(
                ".agents/projects/_projects.md",
                Document(
                    "Projects",
                    "Project",
                    "# Projects\n\n## Entries\n\n"
                    + "<!-- open-forge:generated-index:start -->\n"
                    + "- [Guide](guide.md) - #Guide\n"
                    + "- [Linked](linked.md) - #Guide\n"
                    + "- [Broken](broken.md) - #Guide\n"
                    + "<!-- open-forge:generated-index:end -->\n"));
            workspace.WriteText(
                ".agents/projects/guide.md",
                Document(
                    "Guide",
                    "Guide",
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
