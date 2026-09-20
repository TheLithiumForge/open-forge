using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

public sealed class RepairSafetyIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Repair automatic dry-run previews one safe-exact effect and leaves workspace, lock, "
        + "recovery, and temporary state unchanged"),
     Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task AutomaticDryRunHasNoPersistentEffects()
    {
        using var workspace = RepairIntegrationWorkspace.Create(
            "repair-automatic-dry-run",
            includeSafeExact: true,
            includeGuided: true);
        var before = workspace.SnapshotState();
        var sourceBefore = workspace.ReadText(RepairIntegrationWorkspace.SourcePath);

        var result = await ExecuteAsync(
            workspace.Request(
                mode: RepairMode.DryRun,
                automatic: true));

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(RepairMode.DryRun, result.Mode);
        Assert.NotNull(result.Selection);
        Assert.Single(result.Selection!.Selected);
        Assert.Single(result.Selection.Unselected);
        Assert.NotNull(result.Plan);
        Assert.Single(result.Plan!.Effects);
        Assert.Empty(result.Plan.Conflicts);
        Assert.Equal(RepairPreflightState.Ready, result.Preflight.State);
        Assert.Equal(RepairApplicationState.NotRequested, result.Application.State);
        Assert.Equal(RepairRecoveryState.NotCreated, result.Recovery.State);
        Assert.Equal(RepairVerificationState.NotRequested, result.Verification.Targets);
        Assert.Equal(RepairVerificationState.NotRequested, result.Verification.ResultingBytes);
        Assert.Equal(RepairVerificationState.NotRequested, result.Verification.PostConditions);
        Assert.Equal(sourceBefore, workspace.ReadText(RepairIntegrationWorkspace.SourcePath));
        Assert.Equal(before, workspace.SnapshotState());
        workspace.AssertNoWriteInfrastructure();
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Repair without automatic, explicit, or interactive authority blocks before any "
        + "workspace effect"),
     Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task MissingSelectionAuthorityBlocksWithoutWrites()
    {
        using var workspace = RepairIntegrationWorkspace.Create(
            "repair-missing-selection-authority",
            includeSafeExact: true,
            includeGuided: true);
        var before = workspace.SnapshotState();

        var result = await ExecuteAsync(
            workspace.Request(
                mode: RepairMode.Apply,
                automatic: false,
                allowInteraction: false));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RepairFindingCode.SelectionRequired);
        Assert.Null(result.Selection);
        Assert.Null(result.Plan);
        Assert.Equal(RepairApplicationState.NotRequested, result.Application.State);
        Assert.Equal(RepairRecoveryState.NotRequired, result.Recovery.State);
        Assert.Equal(before, workspace.SnapshotState());
        workspace.AssertNoWriteInfrastructure();
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Repair stale explicit destination blocks the complete plan before recovery or writes"),
     Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task StaleExplicitDestinationBlocksBeforeWrites()
    {
        using var workspace = RepairIntegrationWorkspace.Create(
            "repair-stale-explicit-destination",
            includeSafeExact: false,
            includeGuided: true);
        var before = workspace.SnapshotState();

        var result = await ExecuteAsync(
            workspace.Request(
                relinks: [workspace.GuidedRelink(expectedDestination: "changed.md")]));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RepairFindingCode.ProposalUnavailable);
        Assert.Equal(RepairApplicationState.NotRequested, result.Application.State);
        Assert.Equal(RepairRecoveryState.NotRequired, result.Recovery.State);
        Assert.Equal(before, workspace.SnapshotState());
        workspace.AssertNoWriteInfrastructure();
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Repair disappearing explicit target blocks without selecting a fallback or "
        + "partially applying"),
     Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task DisappearingExplicitTargetBlocksWithoutFallback()
    {
        using var workspace = RepairIntegrationWorkspace.Create(
            "repair-disappearing-explicit-target",
            includeSafeExact: false,
            includeGuided: true);
        var relink = workspace.GuidedRelink();
        File.Delete(workspace.Combine(RepairIntegrationWorkspace.GuidedTargetPath));
        var before = workspace.SnapshotState();

        var result = await ExecuteAsync(
            workspace.Request(relinks: [relink]));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RepairFindingCode.ProposalUnavailable);
        Assert.Equal(RepairApplicationState.NotRequested, result.Application.State);
        Assert.Equal(RepairRecoveryState.NotRequired, result.Recovery.State);
        Assert.Equal(before, workspace.SnapshotState());
        workspace.AssertNoWriteInfrastructure();
    }

    private static async Task<RepairResult> ExecuteAsync(
        RepairRequest request)
        => await new RepairOperation().ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);
}
