using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedIndexProcessTests
{
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
            ["index", PublishedIndexWorkspace.RootPath, "--dry-run", "--dry-run", "--detail", "standard"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("Would update the Entries section in 1 of 1 file.", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "--- .agents/root/_root.md  (Entries section)",
            result.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("- - stale", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains($"+ {PublishedIndexWorkspace.ExpectedEntry}", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("No files were changed.", result.StandardOutput, StringComparison.Ordinal);
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
        Assert.Contains("Updated the Entries section in 1 of 1 file.", first.StandardOutput, StringComparison.Ordinal);
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
        Assert.Contains("The Entries section is current in 1 file. Nothing to do.", second.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Nothing to do.", second.StandardOutput, StringComparison.Ordinal);
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
            ["index", ".agents/../private.md", "--format", "json"],
            workspace.ProcessEnvironment);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.DoesNotContain("private.md", result.StandardOutput, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("invalid-input", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            "index.invalid-source",
            document.RootElement.GetProperty("findings")[0].GetProperty("code").GetString());
        workspace.AssertNoLockInfrastructure();
    }
}
