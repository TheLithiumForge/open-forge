using OpenForge.Cli.Core.Commands.Route.Create;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create;

public sealed class RouteCreateApplicationIntegrationTests
{
    public enum RevalidationSubject
    {
        Target,
        Parent,
    }

    [Theory(DisplayName = "Route Create revalidation detects concurrent target or parent changes"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    [InlineData(RevalidationSubject.Target)]
    [InlineData(RevalidationSubject.Parent)]
    public static async Task RevalidationDetectsConcurrentPlanChanges(
        RevalidationSubject subject)
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            $"route-create-revalidate-{subject.ToString().ToLowerInvariant()}");
        workspace.SeedBase();
        var plan = await workspace.BuildPlanAsync();
        ChangePlannedSubject(workspace, subject);

        var result = await new RouteCreatePlanRevalidator(new RouteCreatePlanBuilder())
            .RevalidateAsync(plan, TestContext.Current.CancellationToken);

        Assert.Equal(RouteCreatePlanRevalidationState.Changed, result.State);
    }

    [Fact(DisplayName = "Route Create application reports ordered destination and generated-navigation effects"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task ApplicationReportsOrderedVerifiedEffects()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            "route-create-application-order");
        workspace.SeedBase();
        var plan = await workspace.BuildPlanAsync();
        workspace.OwnApplicationCreatedTarget();

        var formation = await new RouteCreateApplicationOperation(workspace.LockStoreRoot)
            .ExecuteAsync(plan, TestContext.Current.CancellationToken);

        Assert.Equal(RouteCreateVerificationState.Verified, formation.Verification);
        Assert.Collection(
            formation.Effects,
            target =>
            {
                Assert.Equal(RouteCreateIntegrationWorkspace.TargetPath, target.Path);
                Assert.Equal(RouteCreateEffectOutcome.Verified, target.Outcome);
            },
            parent =>
            {
                Assert.Equal(RouteCreateIntegrationWorkspace.ParentPath, parent.Path);
                Assert.Equal(RouteCreateEffectOutcome.Verified, parent.Outcome);
            });
        Assert.Equal(
            plan.IntendedTargetBytes.AsSpan().ToArray(),
            File.ReadAllBytes(workspace.Absolute(RouteCreateIntegrationWorkspace.TargetPath)));
        Assert.Contains(
            "[Project overview](overview.md)",
            workspace.ReadText(RouteCreateIntegrationWorkspace.ParentPath),
            StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route Create final verification checks receipts and converged workspace facts"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task FinalVerificationChecksReceiptsAndConvergedWorkspaceFacts()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            "route-create-final-verification");
        workspace.SeedBase();
        var plan = await workspace.BuildPlanAsync();
        var receipts = workspace.ApplyPlan(plan);
        await using var lease = await workspace.AcquireLeaseAsync(Guid.NewGuid());
        var verifier = new RouteCreateAppliedVerifier(
            new RouteCreatePlanBuilder(),
            new FileExpectationValidator(new PhysicalPathResolver()));

        var result = await verifier.VerifyAsync(
            plan,
            lease,
            receipts,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteCreateAppliedVerificationState.Verified, result.State);
    }

    [Fact(DisplayName = "Route Create final verification rejects target bytes changed after application"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task FinalVerificationRejectsPostApplicationTargetChange()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            "route-create-final-verification-change");
        workspace.SeedBase();
        var plan = await workspace.BuildPlanAsync();
        var receipts = workspace.ApplyPlan(plan);
        workspace.ChangeTargetAfterApplication();
        await using var lease = await workspace.AcquireLeaseAsync(Guid.NewGuid());
        var verifier = new RouteCreateAppliedVerifier(
            new RouteCreatePlanBuilder(),
            new FileExpectationValidator(new PhysicalPathResolver()));

        var result = await verifier.VerifyAsync(
            plan,
            lease,
            receipts,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteCreateAppliedVerificationState.Failed, result.State);
    }

    [Fact(DisplayName = "Route Create recovery prepares one verified existing-target bundle"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task RecoveryPreparesOneVerifiedExistingTargetBundle()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            "route-create-recovery-prepare");
        workspace.SeedBase();
        var plan = await workspace.BuildPlanAsync();

        var result = await RouteCreateRecoveryLifecycle.PrepareAsync(
            plan,
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteCreateRecoveryPreparationState.Prepared, result.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(result.Preparation);
        workspace.TrackRecovery(preparation);
        Assert.True(File.Exists(preparation.BundlePath));
    }

    [Fact(DisplayName = "Route Create recovery blocks a deterministic same-operation bundle collision"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task RecoveryBlocksDeterministicSameOperationCollision()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            "route-create-recovery-collision");
        workspace.SeedBase();
        var plan = await workspace.BuildPlanAsync();
        var operationId = Guid.NewGuid();
        var prepared = await RouteCreateRecoveryLifecycle.PrepareAsync(
            plan,
            operationId,
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);

        var collision = await RouteCreateRecoveryLifecycle.PrepareAsync(
            plan,
            operationId,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteCreateRecoveryPreparationState.Blocked, collision.State);
        Assert.Null(collision.Preparation);
        Assert.True(File.Exists(preparation.BundlePath));
    }

    [Fact(DisplayName = "Route Create recovery deletes only the exact verified artifact under its lease"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task RecoveryDeletesOnlyExactVerifiedArtifactUnderLease()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            "route-create-recovery-delete");
        workspace.SeedBase();
        var plan = await workspace.BuildPlanAsync();
        var operationId = Guid.NewGuid();
        var preparation = await workspace.PrepareRecoveryBundleAsync(plan, operationId);
        await using var lease = await workspace.AcquireLeaseAsync(operationId);

        var result = await RouteCreateRecoveryLifecycle.DeleteAsync(
            lease,
            preparation,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteCreateRecoveryState.Removed, result.Recovery.State);
        Assert.False(File.Exists(preparation.BundlePath));
    }

    [Fact(DisplayName = "Route Create recovery retains its verified artifact when deletion is cancelled"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task CancelledRecoveryDeletionRetainsVerifiedArtifact()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            "route-create-recovery-retained");
        workspace.SeedBase();
        var plan = await workspace.BuildPlanAsync();
        var operationId = Guid.NewGuid();
        var preparation = await workspace.PrepareRecoveryBundleAsync(plan, operationId);
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await RouteCreateRecoveryLifecycle.DeleteAsync(
            lease,
            preparation,
            cancellation.Token);

        Assert.Equal(RouteCreateRecoveryState.Retained, result.Recovery.State);
        Assert.Equal(preparation.BundlePath, result.Recovery.ResidualPath);
        Assert.Equal(RouteCreateFindingCode.Interrupted, result.FindingCode);
        Assert.True(File.Exists(preparation.BundlePath));
    }

    [Fact(DisplayName = "Route Create top-level dry-run returns one complete no-write result"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task TopLevelDryRunReturnsCompleteNoWriteResult()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            "route-create-operation-dry-run");
        workspace.SeedBase();
        var before = workspace.SnapshotHashes();
        var parentBefore = workspace.ReadText(RouteCreateIntegrationWorkspace.ParentPath);
        const string targetExpected = "---\nopen-forge:\n  description: Project overview\n  tags: [Docs, Overview]\n  responsibility: Explains the project\n---\n";
        var parentExpected = parentBefore.Replace("- none - No entries - #Empty", "- [Project overview](overview.md) - #Docs #Overview", StringComparison.Ordinal);

        var result = await RouteCreateOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request(RouteCreateMode.DryRun),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal([RouteCreateIntegrationWorkspace.TargetPath, RouteCreateIntegrationWorkspace.ParentPath], result.Effects.Select(effect => effect.Path));
        Assert.Equal([RouteCreateEffectAction.Create, RouteCreateEffectAction.Replace], result.Effects.Select(effect => effect.Action));
        Assert.Null(result.Effects[0].Change?.Before);
        Assert.Equal(Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(targetExpected))), result.Effects[0].Change?.Expected);
        Assert.Equal(Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(parentBefore))), result.Effects[1].Change?.Before);
        Assert.Equal(Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(parentExpected))), result.Effects[1].Change?.Expected);
        Assert.All(result.Effects, effect => Assert.Equal(RouteCreateEffectOutcome.Planned, effect.Outcome));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static void ChangePlannedSubject(
        RouteCreateIntegrationWorkspace workspace,
        RevalidationSubject subject)
    {
        switch (subject)
        {
            case RevalidationSubject.Target:
                workspace.SeedDifferingTarget();
                break;
            case RevalidationSubject.Parent:
                workspace.ChangeParentAfterPlanning();
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(subject),
                    subject,
                    "The Route Create revalidation subject is not defined.");
        }
    }
}
