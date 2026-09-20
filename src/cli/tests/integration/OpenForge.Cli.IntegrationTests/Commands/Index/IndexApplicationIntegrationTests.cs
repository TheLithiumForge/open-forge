using System.Text.Json;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

public sealed class IndexApplicationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact, Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task UnclosedChildMetadataNamesTheActualLeafAndDoesNotMutate()
    {
        using var workspace = IndexOperationWorkspace.Create("index-unclosed-child");
        workspace.ReplaceChildBytes(System.Text.Encoding.UTF8.GetBytes("---\nopen-forge:\n  description: Child\n"));
        var before = workspace.SnapshotHashes();
        var text = await CliHostCapture.RunAsync(["index", IndexOperationWorkspace.RootPath], workspace.Workspace.LexicalRoot);
        Assert.Equal(5, text.ExitCode);
        Assert.Equal(string.Empty, text.Output);
        Assert.Equal(("Cannot rebuild the Entries section of .agents/root/_root.md.\n"
            + $"Workspace: {workspace.Workspace.LexicalRoot}\n"
            + "  Error  .agents/root/child.md:1:1  Frontmatter is invalid\n"
            + "         The frontmatter block is not closed.\n"
            + "Next: open-forge doctor\n").ReplaceLineEndings(Environment.NewLine), text.Error);
        var structured = await CliHostCapture.RunAsync(
            ["index", IndexOperationWorkspace.RootPath, "--format", "json", "--detail", "full"], workspace.Workspace.LexicalRoot);
        Assert.Equal(5, structured.ExitCode);
        Assert.Equal(string.Empty, structured.Error);
        using var json = JsonDocument.Parse(structured.Output);
        var finding = Assert.Single(json.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal("index.metadata-unsafe", finding.GetProperty("code").GetString());
        Assert.Equal(".agents/root/child.md", finding.GetProperty("subject").GetProperty("path").GetString());
        Assert.Equal(1, finding.GetProperty("subject").GetProperty("location").GetProperty("line").GetInt32());
        Assert.Equal(1, finding.GetProperty("subject").GetProperty("location").GetProperty("column").GetInt32());
        Assert.Empty(json.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.SnapshotHashes());
        AssertNoPersistentState(workspace.Workspace);
        Assert.Equal(0, await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "Host")]
    [Theory, InlineData(".agents/root", "root"), InlineData("missing-source", null), InlineData(".agents/../outside.md", null)]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task FolderUnknownAndMalformedOperandsRemainReadOnly(string operand, string? correctedId)
    {
        using var workspace = IndexOperationWorkspace.Create("index-input-facts");
        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(["index", operand, "--format", "json"], workspace.Workspace.LexicalRoot);
        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var json = JsonDocument.Parse(result.Output);
        Assert.Equal("invalid-input", json.RootElement.GetProperty("status").GetString());
        Assert.Equal("index.invalid-source", Assert.Single(json.RootElement.GetProperty("findings").EnumerateArray()).GetProperty("code").GetString());
        if (correctedId is not null)
        {
            Assert.Equal("Cannot index .agents/root: it is a folder, not a source.", json.RootElement.GetProperty("summary").GetProperty("headline").GetString());
            Assert.Equal("open-forge index root", json.RootElement.GetProperty("next").GetProperty("command").GetString());
        }
        else if (operand == "missing-source")
        {
            Assert.Equal("No source has the ID missing-source.", json.RootElement.GetProperty("findings")[0].GetProperty("message").GetString());
        }
        Assert.Equal(before, workspace.SnapshotHashes());
        AssertNoPersistentState(workspace.Workspace);
        Assert.Equal(0, await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Index help and version bypass workspace resolution and root help registers one leaf"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task TerminalModesBypassWorkspaceAndRootRegistersIndexOnce()
    {
        using var workspace = IndexOperationWorkspace.Create("index-application-terminal");
        var before = workspace.SnapshotHashes();
        AssertNoPersistentState(workspace.Workspace);
        var missing = Path.Combine(workspace.Workspace.LexicalRoot, "missing");
        var root = await CliHostCapture.RunAsync(["--help"], workspace.Workspace.LexicalRoot);
        Assert.Equal(before, workspace.SnapshotHashes());
        AssertNoPersistentState(workspace.Workspace);
        var help = await CliHostCapture.RunAsync(
            ["index", "--help", "--workspace", missing],
            workspace.Workspace.LexicalRoot);
        Assert.Equal(before, workspace.SnapshotHashes());
        AssertNoPersistentState(workspace.Workspace);
        var version = await CliHostCapture.RunAsync(
            ["index", "--version", "--workspace", missing],
            workspace.Workspace.LexicalRoot);
        Assert.Equal(before, workspace.SnapshotHashes());
        AssertNoPersistentState(workspace.Workspace);

        Assert.Equal(0, root.ExitCode);
        Assert.Equal(string.Empty, root.Error);
        Assert.Single(
            root.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries),
            line => line.TrimStart().StartsWith("index", StringComparison.Ordinal));
        Assert.Equal(0, help.ExitCode);
        Assert.Equal(string.Empty, help.Error);
        Assert.Contains("open-forge index [source-reference...] [--dry-run]", help.Output, StringComparison.Ordinal);
        var normalizedHelp = string.Join(' ', help.Output.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        Assert.Contains("--detail <minimal|standard|full|debug>", normalizedHelp, StringComparison.Ordinal);
        Assert.Contains("--detail-filter <error|warning|info|all>", normalizedHelp, StringComparison.Ordinal);
        Assert.Equal(0, version.ExitCode);
        Assert.Equal(string.Empty, version.Error);
        Assert.False(Directory.Exists(missing));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Index repeated dry-run emits every exact bounded diff on stdout without writes"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task RepeatedDryRunEmitsExactCompleteDiffWithoutEffects()
    {
        using var workspace = IndexOperationWorkspace.Create("index-application-dry-run");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["index", IndexOperationWorkspace.RootPath, "--dry-run", "--dry-run", "--detail", "standard"],
            workspace.Workspace.LexicalRoot);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("Would update the Entries section in 1 of 1 file.", result.Output, StringComparison.Ordinal);
        Assert.Contains(
            "--- .agents/root/_root.md  (Entries section)",
            result.Output,
            StringComparison.Ordinal);
        Assert.Contains("- stale", result.Output, StringComparison.Ordinal);
        Assert.Contains($"+ {IndexOperationWorkspace.ExpectedEntry}", result.Output, StringComparison.Ordinal);
        Assert.Contains("No files were changed.", result.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Index JSON retains its complete receipt across views and verbose diagnostics stay bounded on stderr"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task JsonViewAndVerboseDoNotChangePrimaryResult()
    {
        using var workspace = IndexOperationWorkspace.Create("index-application-json");
        var before = workspace.SnapshotHashes();
        AssertNoPersistentState(workspace.Workspace);
        string[] common = ["index", IndexOperationWorkspace.RootPath, "--dry-run", "--format", "json"];
        var compact = await CliHostCapture.RunAsync([.. common, "--detail", "minimal"], workspace.Workspace.LexicalRoot);
        Assert.Equal(before, workspace.SnapshotHashes());
        AssertNoPersistentState(workspace.Workspace);
        var expanded = await CliHostCapture.RunAsync([.. common, "--detail", "standard"], workspace.Workspace.LexicalRoot);
        Assert.Equal(before, workspace.SnapshotHashes());
        AssertNoPersistentState(workspace.Workspace);
        var verbose = await CliHostCapture.RunAsync([.. common, "--detail", "debug"], workspace.Workspace.LexicalRoot);
        Assert.Equal(before, workspace.SnapshotHashes());
        AssertNoPersistentState(workspace.Workspace);

        Assert.Equal(0, compact.ExitCode);
        Assert.Equal(compact.ExitCode, expanded.ExitCode);
        Assert.Equal(compact.ExitCode, verbose.ExitCode);
        using var minimalJson = JsonDocument.Parse(compact.Output);
        using var standardJson = JsonDocument.Parse(expanded.Output);
        using var debugJson = JsonDocument.Parse(verbose.Output);
        foreach (var value in new[] { standardJson.RootElement, debugJson.RootElement })
        {
            Assert.Equal(minimalJson.RootElement.GetProperty("counts").GetRawText(), value.GetProperty("counts").GetRawText());
            Assert.Equal(minimalJson.RootElement.GetProperty("data").GetProperty("changes")[0].GetProperty("path").GetString(),
                value.GetProperty("data").GetProperty("changes")[0].GetProperty("path").GetString());
        }
        Assert.Equal(string.Empty, compact.Error);
        Assert.Equal(string.Empty, expanded.Error);
        Assert.InRange(verbose.Error.Length, 1, 4096);
        Assert.EndsWith(Environment.NewLine, verbose.Error, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(compact.Output);
        Assert.Equal("index", document.RootElement.GetProperty("command").GetString());
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("dry-run", document.RootElement.GetProperty("data").GetProperty("mode").GetString());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Index applies only bounded bytes and a second run is a verified no-op"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task ApplyPreservesOutsideBytesAndSecondRunIsNoOp()
    {
        using var workspace = IndexOperationWorkspace.Create("index-application-apply");

        var first = await CliHostCapture.RunAsync(
            ["index", IndexOperationWorkspace.RootPath],
            workspace.Workspace.LexicalRoot);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(string.Empty, first.Error);
        Assert.Contains("Updated the Entries section in 1 of 1 file.", first.Output, StringComparison.Ordinal);
        Assert.Equal(
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = IndexOperationWorkspace.ExpectedEntry,
                Prefix = IndexOperationWorkspace.RootPrefix,
            }),
            await workspace.ReadRootAsync(TestContext.Current.CancellationToken));
        var afterFirst = workspace.SnapshotHashes();

        var second = await CliHostCapture.RunAsync(
            ["index", IndexOperationWorkspace.RootPath],
            workspace.Workspace.LexicalRoot);

        Assert.Equal(0, second.ExitCode);
        Assert.Equal(string.Empty, second.Error);
        Assert.Contains("The Entries section is current in 1 file. Nothing to do.", second.Output, StringComparison.Ordinal);
        Assert.Contains("Nothing to do.", second.Output, StringComparison.Ordinal);
        Assert.Equal(afterFirst, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Composed Index returns typed JSON errors on stdout without leaking raw operands"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration"),
     InlineData("invalid-source", 4, "invalid-input", "index.invalid-source"),
     InlineData("missing-workspace", 5, "blocked", "index.workspace-unavailable")]
    public static async Task TypedErrorsUseJsonStdout(
        string scenario,
        int expectedExit,
        string expectedStatus,
        string expectedFinding)
    {
        using var workspace = IndexOperationWorkspace.Create("index-application-error");
        string[] arguments = scenario switch
        {
            "invalid-source" => ["index", ".agents/../private.md", "--format", "json"],
            "missing-workspace" => [
                "index", "--workspace", Path.Combine(workspace.Workspace.LexicalRoot, "missing"), "--format", "json",
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The Index application case is not defined."),
        };

        var result = await CliHostCapture.RunAsync(arguments, workspace.Workspace.LexicalRoot);
        var human = await CliHostCapture.RunAsync([.. arguments[..^2], "--detail", "full"], workspace.Workspace.LexicalRoot);

        Assert.Equal(expectedExit, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.DoesNotContain("private.md", result.Output, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal(expectedStatus, document.RootElement.GetProperty("status").GetString());
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == expectedFinding);
        Assert.Equal(expectedExit, human.ExitCode);
        Assert.Equal(string.Empty, human.Output);
        Assert.StartsWith("Cannot ", human.Error);
        Assert.Contains(expectedFinding, human.Error, StringComparison.Ordinal);
        Assert.DoesNotContain("private.md", human.Error, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Index removes retired guards within heading ownership and preserves outside bytes on both runs")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task RetiredGuardsMigrateOnce(string lineEnding)
    {
        using var workspace = IndexOperationWorkspace.Create("index-guard-migration");
        const string prefix = "# Root 😀\n\nAuthored prefix.\n\n## Entries";
        const string suffix = "## Notes\n\nAuthored suffix.\n";
        const string legacy = "\n\n<!-- open-forge:generated-index:start -->\n\n- stale\n\n<!-- open-forge:generated-index:end -->\n\n";
        var before = (prefix + legacy + suffix).Replace("\n", lineEnding, StringComparison.Ordinal);
        workspace.ReplaceRootText(before);
        var first = await CliHostCapture.RunAsync(["index", IndexOperationWorkspace.RootPath, "--format", "json"], workspace.Workspace.LexicalRoot);
        Assert.Equal(0, first.ExitCode);
        var expected = (prefix + "\n\n\n\n" + IndexOperationWorkspace.ExpectedEntry + "\n\n\n\n" + suffix).Replace("\n", lineEnding, StringComparison.Ordinal);
        Assert.Equal(expected, await workspace.ReadRootAsync(TestContext.Current.CancellationToken));
        Assert.DoesNotContain("generated-index", expected, StringComparison.Ordinal);
        var hashes = workspace.SnapshotHashes();
        var second = await CliHostCapture.RunAsync(["index", IndexOperationWorkspace.RootPath], workspace.Workspace.LexicalRoot);
        Assert.Equal(0, second.ExitCode);
        Assert.Contains("Nothing to do.", second.Output, StringComparison.Ordinal);
        Assert.Equal(hashes, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Index refuses duplicate Entries headings without changing workspace bytes")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task DuplicateEntriesPreventWrites()
    {
        using var workspace = IndexOperationWorkspace.Create("index-duplicate-entries");
        workspace.ReplaceRootText("# Root\n\n## Entries\n\nstale\n\n## Entries\n\nother\n");
        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(["index", IndexOperationWorkspace.RootPath, "--format", "json"], workspace.Workspace.LexicalRoot);
        Assert.NotEqual(0, result.ExitCode);
        using var json = JsonDocument.Parse(result.Output);
        Assert.Contains(json.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "index.generated-region-unsafe");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static void AssertNoPersistentState(CliWorkspace workspace)
    {
        var lockStore = WorkspaceLockStoreRoot.ResolveForCurrentUser(Environment.SpecialFolderOption.DoNotVerify);
        Assert.NotNull(lockStore);
        AssertAbsent(WorkspaceLockPathIdentity.LockPath(lockStore, workspace));
        var recoveryStore = RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.DoNotVerify);
        Assert.NotNull(recoveryStore);
        AssertAbsent(RecoveryBundlePathIdentity.WorkspaceDirectory(recoveryStore, workspace.PhysicalRoot));
    }

    private static void AssertAbsent(string absolutePath)
    {
        try
        {
            _ = File.GetAttributes(absolutePath);
            Assert.Fail($"The unique workspace artifact must remain absent: {absolutePath}");
        }
        catch (FileNotFoundException)
        {
        }
        catch (DirectoryNotFoundException)
        {
        }
    }
}
