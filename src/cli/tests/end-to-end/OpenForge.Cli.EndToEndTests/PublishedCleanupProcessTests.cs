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
        Assert.Contains("Recovery-data cleanup", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Removed and verified", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Status: complete", applied.StandardOutput, StringComparison.Ordinal);
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
        Assert.Contains("Status: complete", repeated.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Removed and verified", repeated.StandardOutput, StringComparison.Ordinal);
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
            ["cleanup", "--dry-run", "--json"],
            workspace.ProcessEnvironment);
        var second = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["cleanup", "--dry-run", "--json"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(string.Empty, first.StandardError);
        Assert.Equal(first.ExitCode, second.ExitCode);
        Assert.Equal(first.StandardError, second.StandardError);
        Assert.Equal(first.StandardOutput, second.StandardOutput);

        using var document = JsonDocument.Parse(first.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("cleanup", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(workspace.Path, root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal("current-directory", root.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);

        var cleanup = root.GetProperty("result");
        Assert.Equal(
            ["mode", "catalogue", "plan", "preflight", "lease", "revalidation", "effects", "residuals", "verification", "findings"],
            cleanup.EnumerateObject().Select(property => property.Name));
        Assert.Equal("dry-run", cleanup.GetProperty("mode").GetString());
        Assert.Equal("complete", cleanup.GetProperty("catalogue").GetProperty("coverage").GetString());
        Assert.Equal("safe", cleanup.GetProperty("plan").GetProperty("safety").GetString());
        Assert.Equal("not-requested", cleanup.GetProperty("lease").GetProperty("state").GetString());
        Assert.Equal("not-requested", cleanup.GetProperty("revalidation").GetProperty("state").GetString());
        Assert.Equal("not-requested", cleanup.GetProperty("verification").GetProperty("state").GetString());
        Assert.Empty(cleanup.GetProperty("findings").EnumerateArray());

        var candidates = cleanup.GetProperty("catalogue").GetProperty("candidates").EnumerateArray().ToArray();
        Assert.Equal(2, candidates.Length);
        Assert.Equal(workspace.EligibleFinalPath, candidates[0].GetProperty("path").GetString());
        Assert.Equal("final", candidates[0].GetProperty("kind").GetString());
        Assert.Equal("verified", candidates[0].GetProperty("integrity").GetString());
        Assert.Equal("eligible", candidates[0].GetProperty("eligibility").GetString());
        Assert.Equal("delete", candidates[0].GetProperty("action").GetString());
        Assert.Equal(workspace.EligibleDraftPath, candidates[1].GetProperty("path").GetString());
        Assert.Equal("draft", candidates[1].GetProperty("kind").GetString());
        Assert.Equal("incomplete", candidates[1].GetProperty("integrity").GetString());
        Assert.Equal("eligible", candidates[1].GetProperty("eligibility").GetString());
        Assert.Equal("delete", candidates[1].GetProperty("action").GetString());

        var planEntries = cleanup.GetProperty("plan").GetProperty("entries").EnumerateArray().ToArray();
        Assert.Equal(2, planEntries.Length);
        Assert.Equal(workspace.EligibleFinalPath, planEntries[0].GetProperty("path").GetString());
        Assert.Equal(workspace.EligibleDraftPath, planEntries[1].GetProperty("path").GetString());
        Assert.All(
            planEntries,
            entry =>
            {
                Assert.Equal("planned", entry.GetProperty("resultEffect").GetProperty("outcome").GetString());
                Assert.Equal("none", entry.GetProperty("resultEffect").GetProperty("residual").GetString());
            });

        var effects = cleanup.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Equal(2, effects.Length);
        Assert.All(
            effects,
            effect =>
            {
                Assert.Equal("planned", effect.GetProperty("outcome").GetString());
                Assert.Equal("none", effect.GetProperty("residual").GetString());
            });
        Assert.True(File.Exists(workspace.EligibleFinalPath));
        Assert.True(File.Exists(workspace.EligibleDraftPath));
        Assert.Equal(nestedSentinelHash, PublishedCleanupWorkspace.HashFile(workspace.NestedSentinelPath));
        PublishedCleanupWorkspace.AssertNestedSupportSnapshot(workspace.SnapshotState(), nestedSentinelHash);
        workspace.AssertNoLockInfrastructure();
    }
}
