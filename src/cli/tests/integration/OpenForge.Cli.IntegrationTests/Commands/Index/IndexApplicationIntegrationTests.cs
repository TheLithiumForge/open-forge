using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

public sealed class IndexApplicationIntegrationTests
{
    [Fact(DisplayName = "Composed Index help and version bypass workspace resolution and root help registers one leaf"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task TerminalModesBypassWorkspaceAndRootRegistersIndexOnce()
    {
        using var workspace = IndexOperationWorkspace.Create("index-application-terminal");
        var missing = Path.Combine(workspace.Workspace.LexicalRoot, "missing");
        var root = await CliHostCapture.RunAsync(["--help"], workspace.Workspace.LexicalRoot);
        var help = await CliHostCapture.RunAsync(
            ["index", "--help", "--workspace", missing],
            workspace.Workspace.LexicalRoot);
        var version = await CliHostCapture.RunAsync(
            ["index", "--version", "--workspace", missing],
            workspace.Workspace.LexicalRoot);

        Assert.Equal(0, root.ExitCode);
        Assert.Equal(string.Empty, root.Error);
        Assert.Single(
            root.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries),
            line => line.TrimStart().StartsWith("index", StringComparison.Ordinal));
        Assert.Equal(0, help.ExitCode);
        Assert.Equal(string.Empty, help.Error);
        Assert.Contains("open-forge index [source-reference...] [--dry-run]", help.Output, StringComparison.Ordinal);
        Assert.Contains("--view <compact|expanded>", help.Output, StringComparison.Ordinal);
        Assert.Equal(0, version.ExitCode);
        Assert.Equal(string.Empty, version.Error);
        Assert.False(Directory.Exists(missing));
    }

    [Fact(DisplayName = "Composed Index repeated dry-run emits every exact bounded diff on stdout without writes"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task RepeatedDryRunEmitsExactCompleteDiffWithoutEffects()
    {
        using var workspace = IndexOperationWorkspace.Create("index-application-dry-run");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["index", IndexOperationWorkspace.RootPath, "--dry-run", "--dry-run", "--view", "compact"],
            workspace.Workspace.LexicalRoot);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("Generated Entries would be updated.", result.Output, StringComparison.Ordinal);
        Assert.Contains(
            "@@ {\"id\":\"root\",\"path\":\".agents/root/_root.md\",\"scope\":\"detached\"} @@",
            result.Output,
            StringComparison.Ordinal);
        Assert.Contains("- stale", result.Output, StringComparison.Ordinal);
        Assert.Contains($"+ {IndexOperationWorkspace.ExpectedEntry}", result.Output, StringComparison.Ordinal);
        Assert.Contains("No files changed (--dry-run).", result.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Composed Index JSON is view-neutral and verbose diagnostics stay bounded on stderr"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task JsonViewAndVerboseDoNotChangePrimaryResult()
    {
        using var workspace = IndexOperationWorkspace.Create("index-application-json");
        string[] common = ["index", IndexOperationWorkspace.RootPath, "--dry-run", "--json"];
        var compact = await CliHostCapture.RunAsync([.. common, "--view", "compact"], workspace.Workspace.LexicalRoot);
        var expanded = await CliHostCapture.RunAsync([.. common, "--view", "expanded"], workspace.Workspace.LexicalRoot);
        var verbose = await CliHostCapture.RunAsync([.. common, "--view", "compact", "--verbose"], workspace.Workspace.LexicalRoot);

        Assert.Equal(0, compact.ExitCode);
        Assert.Equal(compact.ExitCode, expanded.ExitCode);
        Assert.Equal(compact.ExitCode, verbose.ExitCode);
        Assert.Equal(compact.Output, expanded.Output);
        Assert.Equal(compact.Output, verbose.Output);
        Assert.Equal(string.Empty, compact.Error);
        Assert.Equal(string.Empty, expanded.Error);
        Assert.InRange(verbose.Error.Length, 1, 4096);
        Assert.EndsWith(Environment.NewLine, verbose.Error, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(compact.Output);
        Assert.Equal("index", document.RootElement.GetProperty("command").GetString());
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("dry-run", document.RootElement.GetProperty("result").GetProperty("mode").GetString());
    }

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
        Assert.Contains("Generated Entries were updated.", first.Output, StringComparison.Ordinal);
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
        Assert.Contains("Generated Entries are up to date.", second.Output, StringComparison.Ordinal);
        Assert.Contains("No files changed.", second.Output, StringComparison.Ordinal);
        Assert.Equal(afterFirst, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Composed Index returns typed JSON errors on stdout without leaking raw operands"),
     Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    [InlineData("invalid-source", 4, "invalid", "index.invalid-source")]
    [InlineData("missing-workspace", 5, "blocked", "index.workspace-unavailable")]
    public async Task TypedErrorsUseJsonStdout(
        string scenario,
        int expectedExit,
        string expectedStatus,
        string expectedFinding)
    {
        using var workspace = IndexOperationWorkspace.Create("index-application-error");
        string[] arguments = scenario switch
        {
            "invalid-source" => ["index", ".agents/../private.md", "--json"],
            "missing-workspace" => [
                "index", "--workspace", Path.Combine(workspace.Workspace.LexicalRoot, "missing"), "--json",
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The Index application case is not defined."),
        };

        var result = await CliHostCapture.RunAsync(arguments, workspace.Workspace.LexicalRoot);
        var human = await CliHostCapture.RunAsync(arguments[..^1], workspace.Workspace.LexicalRoot);

        Assert.Equal(expectedExit, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.DoesNotContain("private.md", result.Output, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal(expectedStatus, document.RootElement.GetProperty("status").GetString());
        Assert.Contains(
            document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == expectedFinding);
        Assert.Equal(expectedExit, human.ExitCode);
        Assert.Equal(string.Empty, human.Output);
        Assert.Contains("Generated Entries were not updated.", human.Error, StringComparison.Ordinal);
        Assert.DoesNotContain("private.md", human.Error, StringComparison.Ordinal);
    }
}
