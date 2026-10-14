using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedIndexProcessTests
{
    [Fact, Trait("Feature", "index-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedNavigationRenameIsOneRowAndKeepsRules()
    {
        using var fixture = new PublishedRepairProcessTests.RenameWorkspace();
        await fixture.InitializeAsync();
        var target = PublishedExecutableTarget.Discover();
        async Task<ProcessRunResult> Run(params string[] arguments)
            => await PublishedProcessTestSupport.RunAsync(target, fixture.Root, [.. arguments, "--workspace", fixture.Root], fixture.EnvironmentVariables);
        Assert.Equal(0, (await Run("index")).ExitCode);
        await fixture.RecordTreeAsync();
        Directory.Move(fixture.PathFor(".agents/guidance/billing"), fixture.PathFor(".agents/guidance/invoices"));
        foreach (var preview in new[] { true, false })
        {
            var output = await Run(preview ? ["index", "--dry-run"] : ["index"]);
            Assert.True(output.ExitCode == 0, output.StandardOutput + output.StandardError);
            Assert.Contains(preview ? "would be renamed from _billing.md, your rules would be kept" : "renamed from _billing.md, your rules were kept", output.StandardOutput);
            Assert.StartsWith(preview ? "Would rename 1 navigation file." : "Renamed 1 navigation file.", output.StandardOutput, StringComparison.Ordinal);
            Assert.DoesNotContain("entrypoint created", output.StandardOutput);
            Assert.DoesNotContain("deleted", output.StandardOutput);
        }
        Assert.Contains("- Keep exact monetary rules.", File.ReadAllText(fixture.PathFor(".agents/guidance/invoices/_invoices.md")));
        Assert.Equal(0, (await Run("repair", "--automatic")).ExitCode);
        Assert.Equal(0, (await Run("doctor")).ExitCode);
    }

    [Theory(DisplayName = "Published Index completes a deep plain or partial note with additive arrays and a byte no-op repeat")]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Feature", "index-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedCompletion(bool partial)
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedIndexWorkspace.CreateRooted();
        workspace.TrackGeneratedFiles(".agents/root/team/_team.md", ".agents/root/team/deep/_deep.md");
        const string path = ".agents/root/team/deep/note.md";
        const string body = "# A note\r\n\r\nKeep my content.\r\n";
        var authored = partial ? "---\r\ndescription: My words # keep comment\r\ncustom: kept\r\n---\r\n\r\n" + body : body;
        workspace.WriteText(path, authored);
        var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState,
            ["index", path, "--dry-run", "--format", "json"], workspace.ProcessEnvironment);
        Assert.Equal(0, preview.ExitCode);
        using (var json = JsonDocument.Parse(preview.StandardOutput))
        {
            var row = Assert.Single(json.RootElement.GetProperty("data").GetProperty("metadata").EnumerateArray());
            Assert.Equal("planned", row.GetProperty("outcome").GetString());
            Assert.Equal(partial ? 1 : 2, row.GetProperty("fields").GetArrayLength());
            Assert.Equal(2, json.RootElement.GetProperty("data").GetProperty("entrypoints").GetArrayLength());
            Assert.Equal(1, json.RootElement.GetProperty("counts").GetProperty("metadataCompleted").GetInt32());
        }
        workspace.AssertNoLockInfrastructure();
        var apply = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, ["index", path], workspace.ProcessEnvironment);
        Assert.Equal(0, apply.ExitCode);
        Assert.Equal(string.Empty, apply.StandardError);
        Assert.Contains("Created 2 entrypoints and completed metadata in 1 file.", apply.StandardOutput, StringComparison.Ordinal);
        var final = workspace.ReadText(path);
        Assert.EndsWith(body, final, StringComparison.Ordinal);
        if (partial) Assert.Contains("description: My words # keep comment\r\ncustom: kept\r\n", final, StringComparison.Ordinal);
        Assert.Contains(partial ? "[My words](note.md) - #Workspace" : "[A note](note.md) - #Workspace",
            workspace.ReadText(".agents/root/team/deep/_deep.md"), StringComparison.Ordinal);
        var repeat = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState,
            ["index", path], workspace.ProcessEnvironment);
        Assert.Equal(0, repeat.ExitCode);
        Assert.Contains("Nothing to do.", repeat.StandardOutput, StringComparison.Ordinal);
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
