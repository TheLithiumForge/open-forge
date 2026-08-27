using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Context;

internal sealed class ContextOperationWorkspace : IDisposable
{
    internal const string GuideFrontmatter = "---\nopen-forge:\n  description: Guide\n  tags: [Guide]\n---\n";
    internal const string GuideBody = "\n# Guide\n\n## Rules\n\nBase rule.\n\n[Linked](linked.md#details) and [External](https://example.com).\n";

    private readonly TemporaryWorkspace _temporary;

    private ContextOperationWorkspace(TemporaryWorkspace temporary)
    {
        _temporary = temporary;
        Workspace = new CliWorkspace(
            temporary.Path,
            temporary.Path,
            CliWorkspaceSelectionMethod.CurrentDirectory);
    }

    internal string Root => _temporary.Path;

    internal CliWorkspace Workspace { get; }

    internal string Absolute(string relativePath)
    {
        return _temporary.Combine(relativePath);
    }

    internal void CreateDirectory(string relativePath)
    {
        _temporary.CreateDirectory(relativePath);
    }

    internal void WriteText(string relativePath, string content)
    {
        _temporary.WriteText(relativePath, content);
    }

    internal void ReplaceText(string relativePath, string content)
    {
        _temporary.ReplaceText(relativePath, content);
    }

    internal void ReplaceBytes(string relativePath, byte[] content)
    {
        _temporary.ReplaceBytes(relativePath, content);
    }

    internal string ReadText(string relativePath)
    {
        return File.ReadAllText(_temporary.Combine(relativePath));
    }

    internal string MoveFile(string sourceRelativePath, string destinationRelativePath)
    {
        return _temporary.MoveFile(sourceRelativePath, destinationRelativePath);
    }

    internal static ContextOperationWorkspace Create(
        bool includeSelectedAncestorLoadNow = false,
        bool startupLinksToSelectedTarget = false)
    {
        var temporary = TemporaryWorkspace.Create("context-operation");
        try
        {
            Write(temporary, "AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
            Write(temporary, ".agents/loader.md", """
            # Loader

            ## Entries

            <!-- open-forge:generated-index:start -->
            - [Docs](docs/_docs.md) - #LoadNow #Core
            - [State](state/_state.md) - #Memory
            - [Projects](projects/_projects.md) - #Project
            <!-- open-forge:generated-index:end -->
            """);
            Write(temporary, ".agents/docs/_docs.md", EntryPoint("Docs", "LoadNow, Core", """
            - [Topic](topic.md) - #LoadNow #Core
            """));
            var topicBody = startupLinksToSelectedTarget
                ? "# Topic\n\nStartup topic. [Shared](../projects/linked.md).\n"
                : "# Topic\n\nStartup topic.\n";
            Write(temporary, ".agents/docs/topic.md", Document("Topic", "LoadNow, Core", topicBody));
            Write(temporary, ".agents/state/_state.md", EntryPoint("State", "Memory", """
            - [Checkpoint](checkpoint.md) - #KeepInMind #Memory
            """));
            Write(temporary, ".agents/state/checkpoint.md", Document("Checkpoint", "KeepInMind, Memory", "# Checkpoint\n\nContinuity.\n"));
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
            Write(temporary, ".agents/projects/_projects.md", EntryPoint("Projects", "Project", projectEntries));
            if (includeSelectedAncestorLoadNow)
            {
                Write(
                    temporary,
                    ".agents/projects/ancestor-load.md",
                    Document("Ancestor load", "LoadNow, Guide", "# Ancestor load\n\nSelected ancestor content.\n"));
            }

            Write(temporary, ".agents/projects/guide.md", GuideFrontmatter + GuideBody);
            Write(temporary, ".agents/projects/guide.overwrite.md", Document("Guide overwrite", "Guide", "# Guide overwrite\n\n## Rules\n\nOverwrite rule.\n"));
            Write(temporary, ".agents/projects/linked.md", Document("Linked", "Guide", "# Linked\n\n## Details\n\nLinked details. [Back](guide.md).\n"));
            return new ContextOperationWorkspace(temporary);
        }
        catch
        {
            temporary.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        _temporary.Dispose();
    }

    private static string EntryPoint(string description, string tags, string entries)
    {
        var navigation = OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Entries = entries.TrimEnd(),
            Prefix = $"# {description}",
            IncludeFinalLineEnding = false,
        });
        return OpenForgeDocumentSeed.Metadata(
            description: description,
            tags: tags.Split(", ", StringSplitOptions.None),
            body: $"\n{navigation}");
    }

    private static string Document(string description, string tags, string body)
        => OpenForgeDocumentSeed.Metadata(
            description: description,
            tags: tags.Split(", ", StringSplitOptions.None),
            body: $"\n{body.TrimEnd()}");

    private static void Write(
        TemporaryWorkspace temporary,
        string relativePath,
        string content)
    {
        temporary.WriteText(
            relativePath,
            content.Replace("\r\n", "\n", StringComparison.Ordinal));
    }
}
