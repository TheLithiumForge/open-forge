using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

public sealed class CleanupPlanningIntegrationTests
{
    [Fact(
        DisplayName = "Cleanup dry-run is deterministic and application retains its catalogue and plan order"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task DryRunAndApplicationShareCatalogueAndPlanOrder()
    {
        using var workspace = CleanupIntegrationWorkspace.Create("cleanup-planning-parity");
        var final = await workspace.AddVerifiedFinalAsync(
            RecoveryBundleProducer.Index,
            RecoveryBundleOperation.Index,
            "cleanup planning final",
            Guid.Parse("55555555-5555-5555-5555-555555555555"));
        var draft = workspace.AddDraft(
            Guid.Parse("66666666-6666-6666-6666-666666666666"));
        var workspaceBefore = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();

        var firstDryRun = await workspace.RunAsync(
            ["cleanup", "--dry-run", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);
        var secondDryRun = await workspace.RunAsync(
            ["cleanup", "--dry-run", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(0, firstDryRun.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, firstDryRun.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, firstDryRun.PrimaryOutputTarget);
        Assert.Equal(string.Empty, firstDryRun.StandardError);
        Assert.Equal(firstDryRun.StandardOutput, secondDryRun.StandardOutput);
        Assert.Equal(firstDryRun.Status, secondDryRun.Status);
        Assert.Equal(firstDryRun.ExitCode, secondDryRun.ExitCode);
        Assert.False(workspace.LockInfrastructureExists);
        CleanupJsonAssertions.AssertNoPersistentEffect(workspace, workspaceBefore, recoveryBefore);

        using var dryDocument = firstDryRun.ParseJson();
        var dryResult = CleanupJsonAssertions.Result(dryDocument);
        Assert.Equal("dry-run", dryResult.GetProperty("mode").GetString());
        Assert.Equal("complete", dryResult.GetProperty("catalogue").GetProperty("coverage").GetString());
        Assert.Equal("safe", dryResult.GetProperty("plan").GetProperty("safety").GetString());
        Assert.Equal("complete", dryResult.GetProperty("preflight").GetProperty("state").GetString());
        Assert.Equal("not-requested", dryResult.GetProperty("lease").GetProperty("state").GetString());
        Assert.Equal("not-requested", dryResult.GetProperty("revalidation").GetProperty("state").GetString());
        Assert.Equal("not-requested", dryResult.GetProperty("verification").GetProperty("state").GetString());
        Assert.Empty(dryResult.GetProperty("findings").EnumerateArray());
        Assert.Equal(
            [final, draft],
            CleanupJsonAssertions.Paths(dryResult.GetProperty("catalogue").GetProperty("candidates")));
        Assert.Equal(
            [final, draft],
            CleanupJsonAssertions.Paths(dryResult.GetProperty("plan").GetProperty("entries")));
        Assert.All(
            dryResult.GetProperty("plan").GetProperty("entries").EnumerateArray(),
            entry =>
            {
                Assert.Equal("planned", entry.GetProperty("resultEffect").GetProperty("outcome").GetString());
                Assert.Equal("none", entry.GetProperty("resultEffect").GetProperty("residual").GetString());
            });
        Assert.All(
            dryResult.GetProperty("effects").EnumerateArray(),
            effect =>
            {
                Assert.Equal("delete", effect.GetProperty("action").GetString());
                Assert.Equal("planned", effect.GetProperty("outcome").GetString());
                Assert.Equal("none", effect.GetProperty("residual").GetString());
            });

        var applied = await workspace.RunAsync(
            ["cleanup", "--workspace", workspace.Path, "--json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, applied.PrimaryOutputTarget);
        Assert.Equal(string.Empty, applied.StandardError);
        using var appliedDocument = applied.ParseJson();
        var appliedResult = CleanupJsonAssertions.Result(appliedDocument);
        Assert.Equal("apply", appliedResult.GetProperty("mode").GetString());
        Assert.Equal("complete", appliedResult.GetProperty("catalogue").GetProperty("coverage").GetString());
        Assert.Equal("safe", appliedResult.GetProperty("plan").GetProperty("safety").GetString());
        Assert.Equal("complete", appliedResult.GetProperty("preflight").GetProperty("state").GetString());
        Assert.Equal("acquired", appliedResult.GetProperty("lease").GetProperty("state").GetString());
        Assert.Equal("matched", appliedResult.GetProperty("revalidation").GetProperty("state").GetString());
        Assert.Equal("verified", appliedResult.GetProperty("verification").GetProperty("state").GetString());
        Assert.Empty(appliedResult.GetProperty("findings").EnumerateArray());
        CleanupJsonAssertions.AssertDryRunApplicationParity(dryResult, appliedResult);
        Assert.Equal(
            [final, draft],
            CleanupJsonAssertions.Paths(appliedResult.GetProperty("catalogue").GetProperty("candidates")));
        Assert.Equal(
            CleanupJsonAssertions.Paths(dryResult.GetProperty("plan").GetProperty("entries")),
            CleanupJsonAssertions.Paths(appliedResult.GetProperty("plan").GetProperty("entries")));
        Assert.All(
            appliedResult.GetProperty("effects").EnumerateArray(),
            effect =>
            {
                Assert.Equal("delete", effect.GetProperty("action").GetString());
                Assert.Equal("verified", effect.GetProperty("outcome").GetString());
                Assert.Equal("none", effect.GetProperty("residual").GetString());
            });
        Assert.Empty(appliedResult.GetProperty("residuals").EnumerateArray());
        Assert.False(File.Exists(final));
        Assert.False(File.Exists(draft));
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.NotEqual(recoveryBefore, workspace.SnapshotRecovery());
        Assert.Empty(workspace.SnapshotRecovery());
        Assert.True(workspace.LockInfrastructureExists);
        Assert.Empty(workspace.SnapshotLockBytes());
    }

    [Fact(
        DisplayName = "Cleanup repeats an empty catalogue as a deterministic no-lease no-op"),
     Trait("Feature", "cleanup-command"),
     Trait("Evidence", "Integration")]
    public async Task EmptyCatalogueIsDeterministicNoLeaseNoOp()
    {
        using var workspace = CleanupIntegrationWorkspace.Create(
            "cleanup-planning-empty",
            withEntry: false);
        var workspaceBefore = workspace.SnapshotWorkspace();
        var recoveryBefore = workspace.SnapshotRecovery();

        var first = await workspace.RunAsync(
            ["cleanup", "--json"],
            TestContext.Current.CancellationToken);
        var second = await workspace.RunAsync(
            ["cleanup", "--json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, first.PrimaryOutputTarget);
        Assert.Equal(string.Empty, first.StandardError);
        Assert.Equal(first.StandardOutput, second.StandardOutput);
        using var document = first.ParseJson();
        Assert.Equal(workspace.Path, document.RootElement.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal("current-directory", document.RootElement.GetProperty("workspace").GetProperty("selectedBy").GetString());
        var result = CleanupJsonAssertions.Result(document);
        Assert.Equal("apply", result.GetProperty("mode").GetString());
        Assert.Equal("complete", result.GetProperty("catalogue").GetProperty("coverage").GetString());
        Assert.Empty(result.GetProperty("catalogue").GetProperty("candidates").EnumerateArray());
        Assert.Equal("safe", result.GetProperty("plan").GetProperty("safety").GetString());
        Assert.Empty(result.GetProperty("plan").GetProperty("entries").EnumerateArray());
        Assert.Equal("not-requested", result.GetProperty("lease").GetProperty("state").GetString());
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Empty(result.GetProperty("residuals").EnumerateArray());
        Assert.Equal("verified", result.GetProperty("verification").GetProperty("state").GetString());
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        CleanupJsonAssertions.AssertNoPersistentEffect(workspace, workspaceBefore, recoveryBefore);
        Assert.False(workspace.LockInfrastructureExists);
        Assert.Empty(workspace.SnapshotLockBytes());
    }
}
