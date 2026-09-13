using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Hosting.Shared.Presentation;

public sealed class CommandColorIntegrationTests
{
    [Theory(DisplayName = "All command views preserve text, JSON, statuses and streams when color is enabled")]
    [Trait("Feature", "cli-color"), Trait("Evidence", "Integration")]
    [InlineData("install", null)]
    [InlineData("update", null)]
    [InlineData("index", null)]
    [InlineData("repair", null)]
    [InlineData("cleanup", null)]
    [InlineData("doctor", null)]
    [InlineData("status", null)]
    [InlineData("find", null)]
    [InlineData("context", null)]
    [InlineData("references", null)]
    [InlineData("route", "list")]
    [InlineData("route", "inspect")]
    [InlineData("route", "create")]
    [InlineData("route", "init")]
    [InlineData("route", "update")]
    [InlineData("route", "move")]
    [InlineData("route", "remove")]
    [InlineData("extension", "list")]
    [InlineData("extension", "inspect")]
    [InlineData("extension", "create")]
    [InlineData("extension", "install")]
    [InlineData("extension", "update")]
    [InlineData("extension", "remove")]
    [InlineData("library", "list")]
    [InlineData("library", "inspect")]
    [InlineData("library", "attach")]
    [InlineData("library", "detach")]
    [InlineData("library", "sync")]
    public async Task ColorOnlyAddsAccents(string command, string? leaf)
    {
        using var workspace = TemporaryWorkspace.Create("command-color");
        var before = workspace.SnapshotHashes();
        string[] path = leaf is null ? [command] : [command, leaf];
        string[] arguments = [.. path, "--workspace", workspace.Combine("missing")];
        var colors = new CliOutputColors(StandardOutput: true, StandardError: true);
        foreach (var view in new[] { "compact", "expanded" })
        {
            var plain = await CliHostCapture.RunAsync([.. arguments, "--view=" + view], workspace.Path);
            var colored = await CliHostCapture.RunAsync([.. arguments, "--view=" + view], workspace.Path, colors);
            Assert.Equal(plain.ExitCode, colored.ExitCode);
            Assert.Equal(plain.Output, RemoveAccents(colored.Output));
            Assert.Equal(plain.Error, RemoveAccents(colored.Error));
            Assert.Contains("\u001b[", colored.Output + colored.Error, StringComparison.Ordinal);
            var jsonPlain = await CliHostCapture.RunAsync([.. arguments, "--view=" + view, "--json"], workspace.Path);
            var jsonColored = await CliHostCapture.RunAsync([.. arguments, "--view=" + view, "--json"], workspace.Path, colors);
            Assert.Equal(jsonPlain, jsonColored);
            Assert.DoesNotContain("\u001b", jsonColored.Output, StringComparison.Ordinal);
        }
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Colour preserves selected authored bodies, Find rows and generated navigation preview diffs"), Trait("Feature", "cli-color"), Trait("Evidence", "Integration")]
    [InlineData("compact")]
    [InlineData("expanded")]
    public async Task ContentAndPreviewStayPlain(string view)
    {
        using var workspace = TemporaryWorkspace.Create("color-content");
        const string body = "# Guide\n\nWARNING Status: Context Next: authored text stays exact.\n";
        workspace.WriteText(".agents/loader.md", "# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Docs](docs/_docs.md)\n<!-- open-forge:generated-index:end -->\n");
        workspace.WriteText(".agents/docs/_docs.md", "---\nopen-forge:\n  description: Docs\n---\n# Docs\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- none - No entries - #Empty\n<!-- open-forge:generated-index:end -->\n");
        workspace.WriteText(".agents/docs/guide.md", "---\nopen-forge:\n  description: Guide\n  tags: [Demo]\n---\n" + body);
        var before = workspace.SnapshotHashes();
        var colors = new CliOutputColors(StandardOutput: true, StandardError: true);
        foreach (string[] command in new string[][]
        {
            ["find", "--tag", "Demo", "--content", "body"],
            ["context", ".agents/docs/guide.md", "--content", "body"],
            ["index", ".agents/docs/_docs.md", "--dry-run"],
        })
        {
            string[] arguments = [.. command, "--workspace", workspace.Path, "--view=" + view];
            var plain = await CliHostCapture.RunAsync(arguments, workspace.Path);
            var colored = await CliHostCapture.RunAsync(arguments, workspace.Path, colors);
            Assert.Equal(command[0] == "find" ? 0 : 3, plain.ExitCode);
            Assert.Equal(plain.ExitCode, colored.ExitCode);
            Assert.Equal(string.Empty, colored.Error);
            Assert.Equal(plain.Output, RemoveAccents(colored.Output));
            Assert.Contains("\u001b[", colored.Output, StringComparison.Ordinal);
            if (command[0] is "find" or "context")
            {
                Assert.Contains(body, colored.Output, StringComparison.Ordinal);
                if (command[0] == "find" && view == "compact")
                {
                    Assert.Contains("docs/guide\t.agents/docs/guide.md", colored.Output, StringComparison.Ordinal);
                }
            }
            else
            {
                var preview = plain.Output.IndexOf("@@ ", StringComparison.Ordinal);
                Assert.True(preview >= 0, plain.Output);
                foreach (var line in plain.Output[preview..].Split('\n').Where(line => line.StartsWith('+') || line.StartsWith('-') || line.StartsWith('@')))
                {
                    Assert.Contains(line, colored.Output, StringComparison.Ordinal);
                }
            }
        }
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static string RemoveAccents(string text)
    {
        foreach (var code in new[] { "31", "32", "33", "36", "39" })
        {
            text = text.Replace($"\u001b[{code}m", string.Empty, StringComparison.Ordinal);
        }
        return text;
    }
}
