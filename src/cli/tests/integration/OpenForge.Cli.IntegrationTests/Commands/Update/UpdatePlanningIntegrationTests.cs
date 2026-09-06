using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

public sealed class UpdatePlanningIntegrationTests
{
    [Fact(DisplayName = "Update real workspace planning builds a trusted no-op for installed state"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task BuildsTrustedNoOpForInstalledWorkspace()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-plan-no-op");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);

        var build = await workspace.BuildAsync(workspace.Request());

        Assert.NotNull(build.Plan);
        Assert.True(build.Plan!.IsNoOp);
        Assert.Equal(CliSemanticStatus.Complete, build.Preview.Status);
        Assert.Empty(build.Preview.Effects);
    }

    [Fact(DisplayName = "Update real workspace planning plans a safe source change for baseline-equivalent content"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task PlansSafeSourceChangeForBaselineEquivalentFile()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-plan-source-change");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.SeedSafePreviousSourceVersion();

        var build = await workspace.BuildAsync(workspace.Request());

        Assert.NotNull(build.Plan);
        Assert.Contains(
            build.Plan!.Decisions,
            decision => decision.Comparison.RelativePath == UpdateIntegrationWorkspace.RetiredCandidatePath
                && decision.Disposition is UpdatePlanningDisposition.Replace);
    }

    [Fact(DisplayName = "Update real workspace planning creates a genuinely new target"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task PlansGenuinelyNewTargetCreation()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-plan-new-target");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.SeedGenuinelyNewSourceTarget();

        var build = await workspace.BuildAsync(workspace.Request());

        Assert.NotNull(build.Plan);
        Assert.Contains(
            build.Plan!.Decisions,
            decision => decision.Comparison.RelativePath == UpdateIntegrationWorkspace.RetiredCandidatePath
                && decision.Disposition is UpdatePlanningDisposition.Create);
    }

    [Fact(DisplayName = "Update real workspace planning preserves changed content with attention"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task PreservesChangedTargetAndReportsAttention()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-plan-divergence");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();

        var build = await workspace.BuildAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Attention, build.Preview.Status);
        Assert.Contains(
            build.Preview.Findings,
            finding => finding.Code.ToString().Contains("ManagedDivergence", StringComparison.Ordinal));
        Assert.Empty(build.Preview.Effects);
    }

    [Fact(DisplayName = "Update real workspace planning preserves a missing target with attention"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task PreservesMissingTargetAndReportsAttention()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-plan-missing-target");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.RemoveManagedContent();

        var build = await workspace.BuildAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Attention, build.Preview.Status);
        Assert.Contains(
            build.Preview.Findings,
            finding => finding.Code.ToString().Contains("ManagedTargetMissing", StringComparison.Ordinal));
        Assert.Empty(build.Preview.Effects);
    }

    [Fact(DisplayName = "Update real workspace planning recognizes historical retired source absent from inventory"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task RecognizesHistoricalRetiredSourceAbsentFromInventory()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-plan-retired-source");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.SeedHistoricalRetiredTarget();

        var build = await workspace.BuildAsync(workspace.Request());

        Assert.NotNull(build.Plan);
        Assert.Contains(
            build.Preview.Comparisons,
            comparison => comparison.RelativePath == UpdateIntegrationWorkspace.HistoricalTargetPath
                && comparison.IntendedState is UpdateComparisonIntendedState.Retired);
        Assert.Empty(build.Preview.Effects);
    }

    [Fact(DisplayName = "Update real workspace planning force replaces and restores without retired deletion"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task PlansForceReplacementAndRestorationWithoutRetiredDeletion()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-plan-force-boundary");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        workspace.RemoveRetiredCandidate();

        var build = await workspace.BuildAsync(workspace.Request(force: true));

        Assert.NotNull(build.Plan);
        Assert.DoesNotContain(
            build.Plan!.Decisions,
            decision => decision.Disposition is UpdatePlanningDisposition.Delete);
        Assert.Contains(
            build.Plan.Decisions,
            decision => decision.Comparison.RelativePath == UpdateIntegrationWorkspace.ManagedPath
                && decision.Disposition is UpdatePlanningDisposition.Replace);
        Assert.Contains(
            build.Plan.Decisions,
            decision => decision.Comparison.RelativePath == UpdateIntegrationWorkspace.RetiredCandidatePath
                && decision.Disposition is UpdatePlanningDisposition.Restore);
    }

    [Fact(DisplayName = "Update real workspace planning prune deletes eligible retired content only"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task PlansPruneDeletionOnlyForEligibleRetiredContent()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-plan-prune-boundary");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.SeedHistoricalRetiredTarget();

        var build = await workspace.BuildAsync(workspace.Request(prune: true));

        Assert.NotNull(build.Plan);
        Assert.Contains(
            build.Plan!.Decisions,
            decision => decision.Comparison.RelativePath == UpdateIntegrationWorkspace.HistoricalTargetPath
                && decision.Disposition is UpdatePlanningDisposition.Delete);
        Assert.DoesNotContain(
            build.Plan.Decisions,
            decision => decision.Disposition is UpdatePlanningDisposition.Replace or UpdatePlanningDisposition.Restore);
    }

    [Fact(DisplayName = "Update real workspace planning blocks the complete plan on one ineligible retirement"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task BlocksCompletePlanOnOneIneligibleRetirement()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-plan-ineligible-retirement");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.SeedHistoricalRetiredTarget();
        workspace.DivergeHistoricalRetiredTarget();

        var build = await workspace.BuildAsync(workspace.Request(prune: true));

        Assert.True(build.Plan is null || build.Plan.IsBlocked);
        Assert.Equal(CliSemanticStatus.Blocked, build.Preview.Status);
        Assert.Empty(build.Preview.Effects);
    }

    [Fact(DisplayName = "Update real workspace planning coalesces generated navigation into one physical effect"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task ProjectsGeneratedNavigationIntoOneCoalescedPhysicalEffect()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-plan-generated-projection");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var intended = workspace.SeedCoalescedAuthoredAndGeneratedChange();

        var build = await workspace.BuildAsync(workspace.Request(force: true));

        Assert.NotNull(build.Preview.GeneratedNavigation);
        var planned = Assert.Single(
            build.Preview.Effects,
            value => value.Path == UpdateIntegrationWorkspace.GeneratedPath);
        Assert.Equal(
            [UpdateComparisonTargetKind.File, UpdateComparisonTargetKind.GeneratedRegion],
            planned.Changes.Select(change => change.Kind));

        var result = await workspace.ExecuteAsync(workspace.Request(force: true));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var applied = Assert.Single(result.Effects);
        Assert.Equal(UpdateIntegrationWorkspace.GeneratedPath, applied.Path);
        Assert.Equal(UpdatePhysicalEffectOutcome.Verified, applied.Outcome);
        Assert.Equal(
            [UpdateComparisonTargetKind.File, UpdateComparisonTargetKind.GeneratedRegion],
            applied.Changes.Select(change => change.Kind));
        Assert.Equal(intended, workspace.ReadBytes(UpdateIntegrationWorkspace.GeneratedPath));
        Assert.Equal(UpdateRecoveryState.Removed, result.Recovery.State);
        Assert.Equal(
            [UpdateIntegrationWorkspace.GeneratedPath, UpdateIntegrationWorkspace.LifecyclePath],
            result.Recovery.ProtectedPaths);
        Assert.Equal(UpdateVerificationState.Verified, result.Verification);
        var afterApplication = workspace.SnapshotHashes();

        var repeat = await workspace.ExecuteAsync(workspace.Request(force: true));

        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Effects);
        Assert.Equal(UpdateVerificationState.Verified, repeat.Verification);
        Assert.Equal(UpdateRecoveryState.NotRequired, repeat.Recovery.State);
        Assert.Equal(afterApplication, workspace.SnapshotHashes());
    }
}
