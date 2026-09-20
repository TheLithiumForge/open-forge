using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

public sealed class CleanupPlanningIntegrationTests
{
    [Trait("Boundary", "OS")]
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
            ["cleanup", "--dry-run", "--workspace", workspace.Path, "--format", "json"],
            TestContext.Current.CancellationToken);
        var secondDryRun = await workspace.RunAsync(
            ["cleanup", "--dry-run", "--workspace", workspace.Path, "--format", "json"],
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
        CleanupJsonAssertions.PropertyOrder(dryResult, "mode", "items");
        Assert.Equal("dry-run", dryResult.GetProperty("mode").GetString());
        Assert.Equal([final, draft], CleanupJsonAssertions.Paths(dryResult.GetProperty("items")));
        Assert.All(
            dryResult.GetProperty("items").EnumerateArray(),
            item => Assert.Equal("would-be-removed", item.GetProperty("outcome").GetString()));

        var dryEffects = dryDocument.RootElement.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Equal(2, dryEffects.Length);
        Assert.All(
            dryEffects,
            effect =>
            {
                Assert.Equal("deleted", effect.GetProperty("action").GetString());
                Assert.Equal("planned", effect.GetProperty("outcome").GetString());
            });

        var applied = await workspace.RunAsync(
            ["cleanup", "--workspace", workspace.Path, "--format", "json"],
            TestContext.Current.CancellationToken);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, applied.PrimaryOutputTarget);
        Assert.Equal(string.Empty, applied.StandardError);
        using var appliedDocument = applied.ParseJson();
        var appliedResult = CleanupJsonAssertions.Result(appliedDocument);
        Assert.Equal("apply", appliedResult.GetProperty("mode").GetString());
        Assert.Equal([final, draft], CleanupJsonAssertions.Paths(appliedResult.GetProperty("items")));
        Assert.All(
            appliedResult.GetProperty("items").EnumerateArray(),
            item => Assert.Equal("removed", item.GetProperty("outcome").GetString()));
        var appliedEffects = appliedDocument.RootElement.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Equal(2, appliedEffects.Length);
        Assert.All(
            appliedEffects,
            effect =>
            {
                Assert.Equal("deleted", effect.GetProperty("action").GetString());
                Assert.Equal("done", effect.GetProperty("outcome").GetString());
            });
        Assert.False(File.Exists(final));
        Assert.False(File.Exists(draft));
        Assert.Equal(workspaceBefore, workspace.SnapshotWorkspace());
        Assert.NotEqual(recoveryBefore, workspace.SnapshotRecovery());
        Assert.Empty(workspace.SnapshotRecovery());
        Assert.True(workspace.LockInfrastructureExists);
        Assert.Empty(workspace.SnapshotLockBytes());
    }

    [Trait("Boundary", "OS")]
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
            ["cleanup", "--format", "json"],
            TestContext.Current.CancellationToken);
        var second = await workspace.RunAsync(
            ["cleanup", "--format", "json"],
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
        Assert.Empty(result.GetProperty("items").EnumerateArray());
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Empty(document.RootElement.GetProperty("findings").EnumerateArray());
        CleanupJsonAssertions.AssertNoPersistentEffect(workspace, workspaceBefore, recoveryBefore);
        Assert.False(workspace.LockInfrastructureExists);
        Assert.Empty(workspace.SnapshotLockBytes());
    }
}
