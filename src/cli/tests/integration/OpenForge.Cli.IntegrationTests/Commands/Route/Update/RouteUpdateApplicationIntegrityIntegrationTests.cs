using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

public sealed class RouteUpdateApplicationIntegrityIntegrationTests
{
    [Fact(DisplayName = "Route Update preserves a verified receipt and rejects a changed later target")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task LaterTargetRaceRetainsExactReceiptAndResidualFacts()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-partial-write-failure");
        var plan = Assert.IsType<RouteUpdatePlan>(
            (await RouteUpdateIntegrationWorkspace.BuildPlanAsync(workspace.Request())).Plan);
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var expectationValidator = new FileExpectationValidator(
            new PhysicalPathResolver());
        var mutationRevalidator = new MutationRevalidator(expectationValidator);
        var validation = await mutationRevalidator.ValidateAsync(
            lease,
            plan.FileChanges,
            TestContext.Current.CancellationToken);
        Assert.Equal(MutationValidationState.Valid, validation.State);
        Assert.Equal(plan.FileChanges.Length, validation.Checks.Count);
        var prepared = await RouteUpdateRecoveryPreparer.PrepareAsync(
            new RouteUpdateRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId.ToString("D"),
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<OpenForge.Cli.Core.Framework.Recovery.Models.Preparation.RecoveryBundlePreparation>(
            prepared.Preparation);
        workspace.TrackRecovery(preparation);
        workspace.ReplaceParentWithDirectory();
        try
        {
            var progress = await new RouteUpdateEffectApplication(
                new FileChangeApplier(
                    mutationRevalidator,
                    expectationValidator)).ApplyAsync(
                new RouteUpdateEffectApplicationInput
                {
                    Plan = plan,
                    Preparation = preparation,
                    Lease = lease,
                    Validation = validation,
                },
                TestContext.Current.CancellationToken);

            Assert.Equal(2, progress.Receipts.Length);
            var targetReceipt = progress.Receipts[0];
            Assert.Equal(
                workspace.Absolute(RouteUpdateIntegrationWorkspace.TargetPath),
                targetReceipt.Change.LogicalPath);
            Assert.Equal(FilesystemEffectState.Applied, targetReceipt.EffectState);
            Assert.Equal(
                FilesystemVerificationState.Verified,
                targetReceipt.VerificationState);
            var parentReceipt = progress.Receipts[1];
            Assert.Equal(
                workspace.Absolute(RouteUpdateIntegrationWorkspace.ParentPath),
                parentReceipt.Change.LogicalPath);
            Assert.Equal(FilesystemEffectState.NotStarted, parentReceipt.EffectState);
            Assert.Equal(
                FilesystemNotStartedReason.TargetChanged,
                parentReceipt.NotStartedReason);
            Assert.Null(progress.UncertainAttempt);
            Assert.Contains(
                progress.Findings,
                finding => finding.Code == RouteUpdateFindingCode.TargetChangedDuringApply);
            Assert.Equal(RouteUpdateRecoveryState.Retained, progress.Recovery.State);
            Assert.Equal(RouteUpdateVerificationState.Unknown, progress.Verification);
            Assert.True(Directory.Exists(workspace.Absolute(
                RouteUpdateIntegrationWorkspace.ParentPath)));
        }
        finally
        {
            workspace.RemoveParentRaceDirectory();
        }
    }

    [Theory(DisplayName = "Route Update semantic verification detects target or overwrite bytes changed after application")]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public static async Task VerificationFailureIsNotReportedAsComplete(bool changeOverwrite)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            changeOverwrite
                ? "route-update-overwrite-verification-failure"
                : "route-update-target-verification-failure");
        if (changeOverwrite)
        {
            workspace.SeedOverwrite();
        }

        var plan = Assert.IsType<RouteUpdatePlan>(
            (await RouteUpdateIntegrationWorkspace.BuildPlanAsync(workspace.Request())).Plan);
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var expectationValidator = new FileExpectationValidator(
            new PhysicalPathResolver());
        var mutationRevalidator = new MutationRevalidator(expectationValidator);
        var validation = await mutationRevalidator.ValidateAsync(
            lease,
            plan.FileChanges,
            TestContext.Current.CancellationToken);
        Assert.Equal(MutationValidationState.Valid, validation.State);
        var prepared = await RouteUpdateRecoveryPreparer.PrepareAsync(
            new RouteUpdateRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId.ToString("D"),
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<OpenForge.Cli.Core.Framework.Recovery.Models.Preparation.RecoveryBundlePreparation>(
            prepared.Preparation);
        workspace.TrackRecovery(preparation);
        var progress = await new RouteUpdateEffectApplication(
            new FileChangeApplier(
                mutationRevalidator,
                expectationValidator)).ApplyAsync(
            new RouteUpdateEffectApplicationInput
            {
                Plan = plan,
                Preparation = preparation,
                Lease = lease,
                Validation = validation,
            },
            TestContext.Current.CancellationToken);
        if (changeOverwrite)
        {
            workspace.MutateOverwriteAfterPlanning();
        }
        else
        {
            workspace.MutateTargetAfterPlanning();
        }

        var verification = await new RouteUpdateAppliedVerifier(
            RouteUpdateIntegrationWorkspace.CreatePlanBuilder(),
            expectationValidator).VerifyAsync(
            new RouteUpdateAppliedVerificationInput
            {
                Plan = plan,
                Progress = progress,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteUpdateAppliedVerificationState.Failed, verification.State);
        Assert.False(string.IsNullOrEmpty(verification.Cause));
    }
}
