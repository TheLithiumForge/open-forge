using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Context;

public sealed class ContextAmbiguousAncestryIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context does not auto-load a Loader root with ambiguous condition ancestors"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task AmbiguousLoaderRootAncestorDoesNotActivateNestedSources()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["context", "--for", "src/Order.cs", "--content=metadata", "--format", "json"],
            workspace.Path);

        Assert.True(
            result.ExitCode == 3,
            $"Expected incomplete exit code 3, got {result.ExitCode}.\nStderr:\n{result.Error}\nStdout:\n{result.Output}");
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("incomplete", root.GetProperty("status").GetString());
        var sources = root.GetProperty("data").GetProperty("sources").EnumerateArray()
            .Select(source => source.GetProperty("path").GetString())
            .ToArray();
        Assert.Equal(["AGENTS.md", ".agents/loader.md"], sources);
        Assert.DoesNotContain(".agents/products/child/_child.md", sources);
        Assert.DoesNotContain(".agents/products/child/leaf.md", sources);
        Assert.Contains(
            root.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "context.applicability-invalid"
                && finding.GetProperty("subject").GetProperty("path").GetString() == ".agents/products/child/_child.md");

        var human = await CliHostCapture.RunAsync(
            ["context", "--for", "src/Order.cs"],
            workspace.Path);

        Assert.Equal(3, human.ExitCode);
        Assert.Contains(
            "A source has invalid applyTo metadata or unresolved route ancestry and was not selected automatically.",
            string.Concat(human.Output, human.Error),
            StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static TemporaryWorkspace CreateWorkspace()
    {
        var workspace = TemporaryWorkspace.Create("context-ambiguous-ancestor");
        try
        {
            workspace.WriteText("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
            workspace.WriteText(
                ".agents/loader.md",
                "# Loader\n\n## Entries\n\n- [Nested](products/child/_child.md) - #LoadNow\n");
            workspace.WriteText(
                ".agents/products/_products.md",
                Document("Products", ["Project"], ["src/**/*.cs"], []));
            workspace.WriteText(
                ".agents/products/index.md",
                Document("Compatibility products", ["Project"], ["tests/**/*.cs"], []));
            workspace.WriteText(
                ".agents/products/child/_child.md",
                Document(
                    "Child",
                    ["Project"],
                    ["src/**/*.cs"],
                    ["- [Leaf](leaf.md)"]));
            workspace.WriteText(
                ".agents/products/child/leaf.md",
                Document("Leaf", ["Rule"], null, []));
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static string Document(
        string description,
        IReadOnlyList<string> tags,
        IReadOnlyList<string>? applyTo,
        IReadOnlyList<string> entries)
    {
        var tagsText = string.Join(", ", tags);
        var applyToLine = applyTo is { Count: > 0 }
            ? $"  applyTo: [{string.Join(", ", applyTo.Select(pattern => $"\"{pattern}\""))}]\n"
            : string.Empty;
        var entryLines = entries.Count == 0
            ? string.Empty
            : $"\n## Entries\n\n{string.Join('\n', entries)}\n";
        return $"---\nopen-forge:\n  description: {description}\n  tags: [{tagsText}]\n{applyToLine}---\n# {description}\n{entryLines}";
    }
}
