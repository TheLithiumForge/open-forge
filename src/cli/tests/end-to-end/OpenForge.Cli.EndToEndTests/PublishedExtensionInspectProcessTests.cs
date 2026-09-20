using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedExtensionInspectProcessTests
{
    [Fact(DisplayName = "Published Extension Inspect help bypasses workspace selection"), Trait("Feature", "extension-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedHelpIsTerminalAndReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInspectWorkspace.Create(
            currentContent: "alpha\n",
            intendedContent: "alpha\n");
        var missingWorkspace = working.Combine("missing-workspace");
        var beforeSource = working.SnapshotSource();

        var help = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            [
                "extension", "inspect", "--help",
                "--workspace", missingWorkspace,
                "--format=json", "--detail=debug", "--detail-filter=warning",
            ]);
        Assert.Equal(0, help.ExitCode);
        Assert.Equal(string.Empty, help.StandardError);
        Assert.Contains("open-forge extension inspect <stable-id>", help.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--source <package-or-catalogue-path>", help.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Results and streams", help.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missingWorkspace));
        Assert.Equal(beforeSource, working.SnapshotSource());
    }

    [Theory(DisplayName = "Published Extension Inspect compares current content with the selected package and ignores leftover state"), Trait("Feature", "extension-inspect"), Trait("Evidence", "EndToEnd")]
    [InlineData("alpha\n", "completed-with-warnings", 2, "changed", "open-forge extension update toolkit --dry-run")]
    [InlineData("beta\n", "completed", 0, "unchanged", "")]
    public static async Task PublishedComparisonUsesCurrentAndIntendedContent(
        string currentContent,
        string expectedStatus,
        int expectedExitCode,
        string expectedRelation,
        string expectedNext)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInspectWorkspace.Create(
            currentContent,
            intendedContent: "beta\n");
        var beforeWorkspace = working.SnapshotState();
        var beforeSource = working.SnapshotSource();

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            [
                "extension", "inspect", "toolkit",
                "--workspace", working.Path,
                "--source", working.SourcePath,
                "--format=json",
            ]);

        Assert.Equal(expectedExitCode, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("extension inspect", root.GetProperty("command").GetString());
        Assert.Equal(expectedStatus, root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("data");
        Assert.Equal("toolkit", commandResult.GetProperty("id").GetString());
        Assert.Equal("package", commandResult.GetProperty("source").GetProperty("kind").GetString());
        Assert.Equal(working.SourcePath, commandResult.GetProperty("source").GetProperty("path").GetString());
        var comparisonPath = Assert.Single(commandResult.GetProperty("files").EnumerateArray());
        Assert.DoesNotContain("baseline", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("current-diverged", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("workspaceBinding", result.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(expectedRelation, comparisonPath.GetProperty("relation").GetString());
        Assert.False(comparisonPath.TryGetProperty("installedSha256", out _));
        Assert.False(comparisonPath.TryGetProperty("packageSha256", out _));
        if (expectedNext.Length == 0)
        {
            Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        }
        else
        {
            Assert.Equal(expectedNext, root.GetProperty("next").GetProperty("command").GetString());
        }

        Assert.Equal(beforeWorkspace, working.SnapshotState());
        Assert.Equal(beforeSource, working.SnapshotSource());
    }


}

internal sealed class PublishedExtensionInspectWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _workspace;
    private readonly TemporaryWorkspace _source;

    private PublishedExtensionInspectWorkspace(
        TemporaryWorkspace workspace,
        TemporaryWorkspace source)
    {
        _workspace = workspace;
        _source = source;
    }

    internal string Path => _workspace.Path;

    internal string SourcePath => _source.Path;

    internal string Combine(params string[] relativeSegments) => _workspace.Combine(relativeSegments);

    internal IReadOnlyDictionary<string, string> SnapshotState() => _workspace.SnapshotHashes();

    internal IReadOnlyDictionary<string, string> SnapshotSource() => _source.SnapshotHashes();

    internal static PublishedExtensionInspectWorkspace Create(
        string currentContent,
        string intendedContent)
    {
        var workspace = TemporaryWorkspace.Create("e2e-extension-inspect-workspace");
        var source = TemporaryWorkspace.Create("e2e-extension-inspect-source");
        try
        {
            workspace.WriteText(".agents/toolkit.md", currentContent);
            workspace.WriteText(
                ".agents/open-forge.lock.json",
                """
                {"schemaVersion":1,"extensions":[{"id":"toolkit","version":"1.0.0","source":"embedded catalogue","dependencies":[],"paths":[".agents/toolkit.md"],"regions":[]}]}
                """);
            workspace.WriteText(".agents/open-forge.lifecycle.json", "{ obsolete and malformed }");
            source.WriteText(
                "extension.json",
                """
                {
                  "id": "toolkit",
                  "name": "Toolkit",
                  "description": "A published test package.",
                  "version": "1.0.0",
                  "dependencies": []
                }
                """);
            source.WriteText("content/.agents/toolkit.md", intendedContent);
            return new PublishedExtensionInspectWorkspace(workspace, source);
        }
        catch
        {
            source.Dispose();
            workspace.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        _source.Dispose();
        _workspace.Dispose();
    }

}
