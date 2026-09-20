using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedCleanupProcessTests
{
    [Fact(DisplayName = "Published Cleanup help is terminal and performs no workspace, recovery, lease, or write work"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "EndToEnd")]
    public async Task HelpIsTerminalAndPerformsNoWorkspaceRecoveryLeaseOrWriteWork()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedCleanupWorkspace.CreateEmpty();
        var missingWorkspace = workspace.Combine("missing-help-workspace");

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["cleanup", "--help", "--workspace", missingWorkspace],
            workspace.ProcessEnvironment);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains(
            "open-forge cleanup [--dry-run] [global options]",
            result.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("Catalogue", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Write policy", result.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missingWorkspace));
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Cleanup removes eligible finals and drafts, preserves blocked and unknown items, and repeats as a no-lease no-op"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "EndToEnd")]
    public async Task ApplicationRemovesEligibleItemsPreservesBlockedAndUnknownThenRepeatsAsNoOp()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedCleanupWorkspace.CreateWithArtifacts();
        var unknownHash = PublishedCleanupWorkspace.HashFile(workspace.UnknownPath);
        var blockedHash = PublishedCleanupWorkspace.HashFile(workspace.ForeignBlockedPath);
        var nestedSentinelHash = PublishedCleanupWorkspace.HashFile(workspace.NestedSentinelPath);
        PublishedCleanupWorkspace.AssertNestedSupportSnapshot(workspace.SnapshotState(), nestedSentinelHash);

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            ["cleanup"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        // The headline states the outcome and there is no `Status:` line. Every removed path is
        // payload, so all of them are listed at minimal detail.
        Assert.Contains("Removed ", applied.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", applied.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Recovery-data cleanup", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(Path.GetFileName(workspace.EligibleFinalPath), applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(Path.GetFileName(workspace.EligibleDraftPath), applied.StandardOutput, StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.EligibleFinalPath));
        Assert.False(File.Exists(workspace.EligibleDraftPath));
        Assert.Equal(unknownHash, PublishedCleanupWorkspace.HashFile(workspace.UnknownPath));
        Assert.Equal(blockedHash, PublishedCleanupWorkspace.HashFile(workspace.ForeignBlockedPath));
        Assert.Equal(nestedSentinelHash, PublishedCleanupWorkspace.HashFile(workspace.NestedSentinelPath));
        PublishedCleanupWorkspace.AssertNestedSupportSnapshot(workspace.SnapshotState(), nestedSentinelHash);
        workspace.AssertPersistentExternalLock();

        using var heldLease = workspace.HoldWorkspaceLease();
        var repeated = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["cleanup"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardError);
        // A second run has nothing left to remove, and a no-op says so.
        Assert.Contains("No recovery data to remove.", repeated.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", repeated.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Removed ", repeated.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(unknownHash, PublishedCleanupWorkspace.HashFile(workspace.UnknownPath));
        Assert.Equal(blockedHash, PublishedCleanupWorkspace.HashFile(workspace.ForeignBlockedPath));
        Assert.Equal(nestedSentinelHash, PublishedCleanupWorkspace.HashFile(workspace.NestedSentinelPath));
        PublishedCleanupWorkspace.AssertNestedSupportSnapshot(workspace.SnapshotState(), nestedSentinelHash);
    }

    [Fact(DisplayName = "Published Cleanup dry-run JSON is deterministic, typed, contingent, and lease-free"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "EndToEnd")]
    public async Task DryRunJsonIsDeterministicTypedContingentAndLeaseFree()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedCleanupWorkspace.CreateWithArtifacts();
        var nestedSentinelHash = PublishedCleanupWorkspace.HashFile(workspace.NestedSentinelPath);
        PublishedCleanupWorkspace.AssertNestedSupportSnapshot(workspace.SnapshotState(), nestedSentinelHash);

        var first = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["cleanup", "--dry-run", "--format=json"],
            workspace.ProcessEnvironment);
        var second = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["cleanup", "--dry-run", "--format=json"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(string.Empty, first.StandardError);
        Assert.Equal(first.ExitCode, second.ExitCode);
        Assert.Equal(first.StandardError, second.StandardError);
        Assert.Equal(first.StandardOutput, second.StandardOutput);

        using var document = JsonDocument.Parse(first.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(
            ["schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("cleanup", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal(workspace.Path, root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal("current-directory", root.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);

        var cleanup = root.GetProperty("data");
        Assert.Equal(["mode", "items"], cleanup.EnumerateObject().Select(property => property.Name));
        Assert.Equal("dry-run", cleanup.GetProperty("mode").GetString());
        var items = cleanup.GetProperty("items").EnumerateArray().ToArray();
        Assert.NotEmpty(items);
        Assert.All(items, item => Assert.Equal(["path", "kind", "outcome"], item.EnumerateObject().Select(member => member.Name)));
        Assert.All(items, item => Assert.Equal("would-be-removed", item.GetProperty("outcome").GetString()));
        Assert.Contains(items, item => item.GetProperty("kind").GetString() == "draft");
        // The dry run names every item it would remove, in catalogue order, and nothing else.
        Assert.Equal(
            [workspace.EligibleFinalPath, workspace.EligibleDraftPath],
            items.Select(item => item.GetProperty("path").GetString()));
        Assert.Equal(["bundle", "draft"], items.Select(item => item.GetProperty("kind").GetString()));

        Assert.True(File.Exists(workspace.EligibleFinalPath));
        Assert.True(File.Exists(workspace.EligibleDraftPath));
        Assert.Equal(nestedSentinelHash, PublishedCleanupWorkspace.HashFile(workspace.NestedSentinelPath));
        PublishedCleanupWorkspace.AssertNestedSupportSnapshot(workspace.SnapshotState(), nestedSentinelHash);
        workspace.AssertNoLockInfrastructure();
    }
}
