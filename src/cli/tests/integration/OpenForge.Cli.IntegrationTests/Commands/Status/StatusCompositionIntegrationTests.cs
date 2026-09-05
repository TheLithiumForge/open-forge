using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class StatusCompositionIntegrationTests
{
    [Fact(DisplayName = "Status preserves a typed non-directory workspace failure through binding"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task NonDirectoryWorkspaceMapsToItsExactStatusFinding()
    {
        using var temporary = TemporaryWorkspace.Create("status-workspace-file");
        var file = temporary.CreateFile("workspace.txt", "not a directory");

        var run = await CliHostCapture.RunAsync(
            ["status", "--workspace", file, "--json"],
            temporary.Path);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(string.Empty, run.Error);
        using var document = JsonDocument.Parse(run.Output);
        var finding = Assert.Single(StatusJsonAssertions.Result(document.RootElement)
            .GetProperty("findings").EnumerateArray());
        Assert.Equal("workspace-not-directory", finding.GetProperty("code").GetString());
    }

    [Fact(DisplayName = "Status preserves a typed unsafe workspace failure through binding"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task UnsafeWorkspaceMapsToItsExactStatusFinding()
    {
        using var temporary = TemporaryWorkspace.Create("status-workspace-unsafe");
        var cycle = temporary.CreateDirectorySymbolicLink("cycle", "cycle");

        var run = await CliHostCapture.RunAsync(
            ["status", "--workspace", cycle, "--json"],
            temporary.Path);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(string.Empty, run.Error);
        using var document = JsonDocument.Parse(run.Output);
        var finding = Assert.Single(StatusJsonAssertions.Result(document.RootElement)
            .GetProperty("findings").EnumerateArray());
        Assert.Equal("workspace-unsafe", finding.GetProperty("code").GetString());
    }

    [Fact(DisplayName = "Root composition registers status between index and context with truthful help"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task RootCompositionRegistersStatusBetweenIndexAndContextWithTruthfulHelp()
    {
        using var workspace = StatusIntegrationWorkspace.Create("status-help");
        workspace.SeedLockBytes([0x91, 0x92]);
        var before = workspace.SnapshotHashes();
        var lockBefore = workspace.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());

        var rootHelp = await StatusIntegrationApplication.RunAsync(workspace, "--help");
        var directLeaves = rootHelp.StandardOutput
            .Split(Environment.NewLine, StringSplitOptions.None)
            .SkipWhile(line => !line.Equals("Commands:", StringComparison.Ordinal))
            .Skip(1)
            .TakeWhile(line => line.StartsWith("  ", StringComparison.Ordinal))
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .Select(line => line.Split(' ', 2)[0])
            .Where(name => name is "find" or "index" or "status" or "context" or "references" or "install")
            .ToArray();
        Assert.Equal(0, rootHelp.ExitCode);
        Assert.Equal(string.Empty, rootHelp.StandardError);
        Assert.Contains("status", directLeaves);
        Assert.Equal(["find", "index", "status", "context", "references", "install"], directLeaves);
        Assert.Equal(1, directLeaves.Count(name => name == "status"));

        var missing = workspace.Combine("missing-status-help");
        var leafHelp = await StatusIntegrationApplication.RunAsync(
            workspace, "status", "--help", "--workspace", missing);
        Assert.Equal(0, leafHelp.ExitCode);
        Assert.Equal(string.Empty, leafHelp.StandardError);
        Assert.Contains("open-forge status [global flags]", leafHelp.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("schemaVersion", leafHelp.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Startup context", leafHelp.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missing));
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
    }

    [Fact(DisplayName = "Source-generated status JSON invocation emits the complete frozen graph with reflection disabled"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task SourceGeneratedStatusJsonInvocationEmitsCompleteFrozenGraphWithReflectionDisabled()
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync("status-generated-json");
        var before = workspace.SnapshotHashes();
        workspace.SeedLockBytes([0xa1, 0xa2, 0xa3]);
        var lockBefore = workspace.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());
        var run = await StatusIntegrationApplication.RunAsync(
            workspace, "status", "--workspace", workspace.Path, "--json");
        Assert.Equal(0, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        StatusJsonAssertions.CompleteGraph(document.RootElement);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
    }
}
