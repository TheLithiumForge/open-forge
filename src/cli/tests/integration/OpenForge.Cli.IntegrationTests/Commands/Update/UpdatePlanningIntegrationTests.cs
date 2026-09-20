using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

public sealed class UpdatePlanningIntegrationTests
{
    [Trait("Boundary", "OS")]
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

    [Trait("Boundary", "OS")]
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

    [Trait("Boundary", "OS")]
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

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Ordinary Update plans replacement or restoration of owned content"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task PlansOwnedContentWithoutForce(bool missing)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-plan-owned-content");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        if (missing) workspace.RemoveManagedContent();
        else workspace.MutateManagedContent();
        var before = workspace.SnapshotHashes();
        var build = await workspace.BuildAsync(workspace.Request());
        Assert.Equal(CliSemanticStatus.Complete, build.Preview.Status);
        Assert.Empty(build.Preview.Findings);
        var decision = Assert.Single(build.Plan!.Decisions, value => value.Comparison.RelativePath == UpdateIntegrationWorkspace.ManagedPath
            && value.Comparison.Kind == UpdateComparisonTargetKind.File);
        Assert.Equal(missing ? UpdatePlanningDisposition.Restore : UpdatePlanningDisposition.Replace, decision.Disposition);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
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
        Assert.DoesNotContain(build.Preview.Effects, effect => effect.Path == UpdateIntegrationWorkspace.HistoricalTargetPath);
    }

    [Trait("Boundary", "OS")]
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

    [Trait("Boundary", "OS")]
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
        Assert.DoesNotContain(build.Plan.Decisions, decision => decision.Disposition == UpdatePlanningDisposition.Delete
            && decision.Comparison.RelativePath != UpdateIntegrationWorkspace.HistoricalTargetPath);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update real workspace planning allows edited retired content inside the destination boundary"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task AllowsEditedRetiredContentInsideBoundary()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-plan-ineligible-retirement");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.SeedHistoricalRetiredTarget();
        workspace.DivergeHistoricalRetiredTarget();

        var build = await workspace.BuildAsync(workspace.Request(prune: true));

        Assert.Equal(CliSemanticStatus.Complete, build.Preview.Status);
        Assert.Contains(build.Preview.Effects, effect => effect.Path == UpdateIntegrationWorkspace.HistoricalTargetPath
            && effect.Action == UpdatePhysicalEffectAction.Delete);
    }

    [Trait("Boundary", "OS")]
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
        Assert.Equal(UpdateRecoveryState.Retained, result.Recovery.State);
        Assert.Equal(
            [UpdateIntegrationWorkspace.GeneratedPath],
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
