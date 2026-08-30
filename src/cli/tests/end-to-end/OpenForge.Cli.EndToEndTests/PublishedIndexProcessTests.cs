using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedIndexProcessTests
{
    [Fact(DisplayName = "Published Index is one direct root leaf whose help bypasses workspace resolution"),
     Trait("Feature", "index-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedIndexHelpUsesExactDirectGrammar()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedIndexWorkspace.Create();
        var root = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["--help"],
            workspace.ProcessEnvironment);
        var help = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["index", "--help", "--workspace", Path.Combine(workspace.Path, "missing")],
            workspace.ProcessEnvironment);

        Assert.Equal(0, root.ExitCode);
        Assert.Equal(string.Empty, root.StandardError);
        Assert.Single(
            root.StandardOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries),
            line => line.TrimStart().StartsWith("index", StringComparison.Ordinal));
        Assert.Equal(0, help.ExitCode);
        Assert.Equal(string.Empty, help.StandardError);
        Assert.Contains("open-forge index [source-reference...] [--dry-run]", help.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--view <compact|expanded>", help.StandardOutput, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Index repeated dry-run emits one exact complete diff and makes no changes"),
     Trait("Feature", "index-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRepeatedDryRunIsExactAndReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedIndexWorkspace.Create();

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["index", PublishedIndexWorkspace.RootPath, "--dry-run", "--dry-run", "--view", "compact"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("Generated Entries would be updated.", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "@@ {\"id\":\"root\",\"path\":\".agents/root/_root.md\",\"scope\":\"detached\"} @@",
            result.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("- stale", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains($"+ {PublishedIndexWorkspace.ExpectedEntry}", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("No files changed (--dry-run).", result.StandardOutput, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Index JSON is view-neutral and verbose diagnostics stay on bounded stderr"),
     Trait("Feature", "index-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedJsonAndDiagnosticsKeepProcessChannelsSeparate()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedIndexWorkspace.Create();
        string[] common = ["index", PublishedIndexWorkspace.RootPath, "--dry-run", "--json"];
        var compact = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            [.. common, "--view", "compact"],
            workspace.ProcessEnvironment);
        var expanded = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            [.. common, "--view", "expanded"],
            workspace.ProcessEnvironment);
        var verbose = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            [.. common, "--view", "compact", "--verbose"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, compact.ExitCode);
        Assert.Equal(compact.ExitCode, expanded.ExitCode);
        Assert.Equal(compact.ExitCode, verbose.ExitCode);
        Assert.Equal(compact.StandardOutput, expanded.StandardOutput);
        Assert.Equal(compact.StandardOutput, verbose.StandardOutput);
        Assert.Equal(string.Empty, compact.StandardError);
        Assert.Equal(string.Empty, expanded.StandardError);
        Assert.InRange(verbose.StandardError.Length, 1, 4096);
        using var document = JsonDocument.Parse(compact.StandardOutput);
        Assert.Equal("index", document.RootElement.GetProperty("command").GetString());
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Index applies one bounded change and the next process run is a verified no-op"),
     Trait("Feature", "index-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedApplyPreservesOutsideBytesAndConverges()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedIndexWorkspace.Create();

        var first = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            ["index", PublishedIndexWorkspace.RootPath],
            workspace.ProcessEnvironment);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(string.Empty, first.StandardError);
        Assert.Contains("Generated Entries were updated.", first.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = PublishedIndexWorkspace.ExpectedEntry,
                Prefix = PublishedIndexWorkspace.RootPrefix,
            }),
            await workspace.ReadRootAsync(TestContext.Current.CancellationToken));
        workspace.AssertPersistentExternalLock();

        var second = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["index", PublishedIndexWorkspace.RootPath],
            workspace.ProcessEnvironment);

        Assert.Equal(0, second.ExitCode);
        Assert.Equal(string.Empty, second.StandardError);
        Assert.Contains("Generated Entries are up to date.", second.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("No files changed.", second.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Index invalid JSON is one typed stdout document with the shared exit"),
     Trait("Feature", "index-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedInvalidJsonUsesTypedStdoutAndSharedExit()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedIndexWorkspace.Create();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["index", ".agents/../private.md", "--json"],
            workspace.ProcessEnvironment);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.DoesNotContain("private.md", result.StandardOutput, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            "index.invalid-source",
            document.RootElement.GetProperty("result").GetProperty("findings")[0].GetProperty("code").GetString());
        workspace.AssertNoLockInfrastructure();
    }
}
