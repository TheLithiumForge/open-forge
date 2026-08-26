using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Commands.Context;

internal sealed class ContextOperationWorkspace : IDisposable
{
    internal const string GuideFrontmatter = "---\nopen-forge:\n  description: Guide\n  tags: [Guide]\n---\n";
    internal const string GuideBody = "\n# Guide\n\n## Rules\n\nBase rule.\n\n[Linked](linked.md#details) and [External](https://example.com).\n";

    private ContextOperationWorkspace(string root)
    {
        Root = root;
        Workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.CurrentDirectory);
    }

    internal string Root { get; }

    internal CliWorkspace Workspace { get; }

    internal static ContextOperationWorkspace Create(
        bool includeSelectedAncestorLoadNow = false,
        bool startupLinksToSelectedTarget = false)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), $"context-operation-{Guid.NewGuid():N}"));
        Directory.CreateDirectory(root);
        Write(root, "AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        Write(root, ".agents/loader.md", """
            # Loader

            ## Entries

            <!-- open-forge:generated-index:start -->
            - [Docs](docs/_docs.md) - #LoadNow #Core
            - [State](state/_state.md) - #Memory
            - [Projects](projects/_projects.md) - #Project
            <!-- open-forge:generated-index:end -->
            """);
        Write(root, ".agents/docs/_docs.md", EntryPoint("Docs", "LoadNow, Core", """
            - [Topic](topic.md) - #LoadNow #Core
            """));
        var topicBody = startupLinksToSelectedTarget
            ? "# Topic\n\nStartup topic. [Shared](../projects/linked.md).\n"
            : "# Topic\n\nStartup topic.\n";
        Write(root, ".agents/docs/topic.md", Document("Topic", "LoadNow, Core", topicBody));
        Write(root, ".agents/state/_state.md", EntryPoint("State", "Memory", """
            - [Checkpoint](checkpoint.md) - #KeepInMind #Memory
            """));
        Write(root, ".agents/state/checkpoint.md", Document("Checkpoint", "KeepInMind, Memory", "# Checkpoint\n\nContinuity.\n"));
        var projectEntries = includeSelectedAncestorLoadNow
            ? """
                - [Guide](guide.md) - #Guide
                - [Ancestor load](ancestor-load.md) - #LoadNow #Guide
                - [Linked](linked.md) - #Guide
                """
            : """
                - [Guide](guide.md) - #Guide
                - [Linked](linked.md) - #Guide
                """;
        Write(root, ".agents/projects/_projects.md", EntryPoint("Projects", "Project", projectEntries));
        if (includeSelectedAncestorLoadNow)
        {
            Write(
                root,
                ".agents/projects/ancestor-load.md",
                Document("Ancestor load", "LoadNow, Guide", "# Ancestor load\n\nSelected ancestor content.\n"));
        }
        Write(root, ".agents/projects/guide.md", GuideFrontmatter + GuideBody);
        Write(root, ".agents/projects/guide.overwrite.md", Document("Guide overwrite", "Guide", "# Guide overwrite\n\n## Rules\n\nOverwrite rule.\n"));
        Write(root, ".agents/projects/linked.md", Document("Linked", "Guide", "# Linked\n\n## Details\n\nLinked details. [Back](guide.md).\n"));
        return new ContextOperationWorkspace(root);
    }

    public void Dispose()
    {
        if (Directory.Exists(Root))
        {
            Directory.Delete(Root, recursive: true);
        }
    }

    private static string EntryPoint(string description, string tags, string entries)
        => $"""
            ---
            open-forge:
              description: {description}
              tags: [{tags}]
            ---

            # {description}

            ## Entries

            <!-- open-forge:generated-index:start -->
            {entries.TrimEnd()}
            <!-- open-forge:generated-index:end -->
            """;

    private static string Document(string description, string tags, string body)
        => $"""
            ---
            open-forge:
              description: {description}
              tags: [{tags}]
            ---

            {body.TrimEnd()}
            """;

    private static void Write(string root, string relativePath, string content)
    {
        var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("A Context test path requires a containing directory.");
        Directory.CreateDirectory(directory);
        File.WriteAllText(path, content.Replace("\r\n", "\n", StringComparison.Ordinal));
    }
}
