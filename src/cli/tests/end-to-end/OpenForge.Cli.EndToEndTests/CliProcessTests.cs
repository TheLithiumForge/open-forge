using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using static OpenForge.Cli.EndToEndTests.Shared.PublishedProcess.PublishedProcessTestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class CliProcessTests
{
    [Fact(DisplayName = "Published version is exact and workspace independent"),
     Trait("Feature", "cli-process"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedVersionIsExactAndWorkspaceIndependent()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = TemporaryWorkspace.Create("e2e-version-working");
        using var other = TemporaryWorkspace.Create("e2e-version-other");
        var missingWorkspace = working.Combine("missing-workspace");
        var workingBefore = working.SnapshotHashes();
        var otherBefore = other.SnapshotHashes();
        var request = new ProcessRunRequest(
            target.ExecutablePath,
            ["--workspace", missingWorkspace, "--json", "--version"],
            other.Path,
            timeout: TimeSpan.FromSeconds(30));

        var result = await ProcessRunner.RunAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(target.ExpectedVersion + Environment.NewLine, result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.False(Directory.Exists(missingWorkspace));
        Assert.Equal(workingBefore, working.SnapshotHashes());
        Assert.Equal(otherBefore, other.SnapshotHashes());
    }

    [Fact(DisplayName = "Published root and Route help expose the current command family"),
     Trait("Feature", "cli-process"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRootAndRouteFamilyHelpExposeAvailableCommands()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = TemporaryWorkspace.Create("e2e-help");

        var root = await RunWithoutWritesAsync(target, working.Path, working.SnapshotHashes, []);
        var help = await RunWithoutWritesAsync(target, working.Path, working.SnapshotHashes, ["--help"]);
        var group = await RunWithoutWritesAsync(target, working.Path, working.SnapshotHashes, ["route"]);
        var leaf = await RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            ["route", "list", "--help"]);
        var inspectLeaf = await RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "--help"]);

        Assert.Equal(0, root.ExitCode);
        Assert.Equal(0, help.ExitCode);
        Assert.Equal(0, group.ExitCode);
        Assert.Equal(0, leaf.ExitCode);
        Assert.Equal(0, inspectLeaf.ExitCode);
        Assert.Equal(root.StandardOutput, help.StandardOutput);
        Assert.Contains("Open Forge CLI (`open-forge`)", root.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Discovery:", root.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("route list", root.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("route inspect", root.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Commands:", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("list <source-reference>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("inspect <source-reference>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("init <route-target>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("create <file-target>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("update <source-reference>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("remove <source-reference>", group.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Operations:", group.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("available —", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "open-forge route list [source-reference] [--depth=<non-negative-integer|all>]",
            leaf.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("open-forge route list memory --depth=all", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "open-forge route inspect <source-reference>",
            inspectLeaf.StandardOutput,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, root.StandardError);
        Assert.Equal(string.Empty, help.StandardError);
        Assert.Equal(string.Empty, group.StandardError);
        Assert.Equal(string.Empty, leaf.StandardError);
        Assert.Equal(string.Empty, inspectLeaf.StandardError);
    }

    [Fact(DisplayName = "Published parser failure uses fixed invalid exit and stderr"),
     Trait("Feature", "cli-process"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedParserFailureUsesFixedInvalidExitAndStandardError()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = TemporaryWorkspace.Create("e2e-invalid");

        var unknown = await RunAsync(target, working.Path, ["--unknown"]);
        var conflict = await RunAsync(target, working.Path, ["--help", "--version"]);

        Assert.Equal(4, unknown.ExitCode);
        Assert.Equal(string.Empty, unknown.StandardOutput);
        Assert.Contains("unknown", unknown.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(4, conflict.ExitCode);
        Assert.Equal(string.Empty, conflict.StandardOutput);
        Assert.Contains("mutually exclusive", conflict.StandardError, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Published process cancellation kills and drains the owned child"),
     Trait("Feature", "cli-process"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedProcessCancellationKillsAndDrainsOwnedChild()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateCancellation(childCount: 500);
        var before = working.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        var startedProcessId = 0;
        var request = new ProcessRunRequest(
            target.ExecutablePath,
            ["route", "list", "root", "--depth=all", "--json"],
            working.Path,
            timeout: TimeSpan.FromSeconds(30),
            processStarted: processId =>
            {
                startedProcessId = processId;
                cancellation.Cancel();
            });

        var exception = await Assert.ThrowsAsync<ProcessRunCanceledException>(() =>
            ProcessRunner.RunAsync(request, cancellation.Token));
        Assert.True(startedProcessId > 0);
        Assert.Equal(startedProcessId, exception.ProcessId);
        Assert.True(exception.KillRequested);
        Assert.Equal(before, working.SnapshotHashes());
    }

}
