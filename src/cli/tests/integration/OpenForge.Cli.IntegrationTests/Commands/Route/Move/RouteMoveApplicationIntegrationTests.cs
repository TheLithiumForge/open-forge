using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

public sealed class RouteMoveApplicationIntegrationTests
{
    [Fact(DisplayName = "Route Move prepares one external ZIP before effects and rejects its exact collision")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task ExternalRecoveryPrecedesEffectsAndCollisionStopsEverything()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-recovery-collision");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var before = workspace.SnapshotHashes();
        var lifecycleBytes = workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath);
        var input = new RouteMoveRecoveryPreparationInput
        {
            Plan = plan,
            OperationId = operationId,
            Lease = lease,
        };

        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);
        var collision = await RouteMoveRecoveryLifecycle.PrepareAsync(
            input,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteMoveRecoveryPreparationState.Prepared, prepared.State);
        Assert.Equal(RouteMoveRecoveryState.NotCreated, prepared.Recovery.State);
        Assert.Null(prepared.Finding);
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.False(Directory.Exists(workspace.Absolute(".agents/archive/application")));
        Assert.Equal(RouteMoveRecoveryPreparationState.Blocked, collision.State);
        Assert.Equal(RouteMoveRecoveryState.Retained, collision.Recovery.State);
        Assert.Equal(preparation.BundlePath, collision.Recovery.ResidualPath);
        Assert.Equal(RouteMoveFindingCode.RecoveryConflict, collision.Finding?.Code);
        Assert.Equal(CliSemanticStatus.Blocked, collision.Finding?.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lifecycleBytes, workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath));
    }

    [Fact(DisplayName = "Route Move reports unavailable external recovery before any effect")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task UnavailableRecoveryStopsBeforeEffects()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-recovery-unavailable");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var blockedPath = workspace.BlockRecoveryWorkspaceDirectory();
        var before = workspace.SnapshotHashes();

        var result = await RouteMoveRecoveryLifecycle.PrepareAsync(
                new RouteMoveRecoveryPreparationInput
                {
                    Plan = plan,
                    OperationId = operationId,
                    Lease = lease,
                },
                TestContext.Current.CancellationToken);

        Assert.True(File.Exists(blockedPath));
        Assert.Equal(RouteMoveRecoveryPreparationState.Incomplete, result.State);
        Assert.Null(result.Preparation);
        Assert.Equal(RouteMoveRecoveryState.Unknown, result.Recovery.State);
        Assert.Equal(RouteMoveFindingCode.RecoveryUnavailable, result.Finding?.Code);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Finding?.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(workspace.Absolute(".agents/archive/application")));
    }

    [Fact(DisplayName = "Route Move keeps ordered heterogeneous receipts and never rolls back after a concrete failure")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task ConcreteFailureLeavesPriorEffectAndAllLaterEffectsNotStarted()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-ordered-failure");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);
        var sourceBefore = workspace.ReadText(RouteMoveIntegrationWorkspace.ApplicationCategoryPath);
        var lifecycleBefore = workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath);
        const string mutatedArchive = "concurrent archive\n";
        workspace.WriteText(".agents/archive/_archive.md", mutatedArchive);

        var progress = await RouteMoveIntegrationWorkspace.CreateEffectApplication()
            .ApplyAsync(
                new RouteMoveEffectApplicationInput
                {
                    Plan = plan,
                    Lease = lease,
                    RecoveryPreparation = prepared,
                },
                TestContext.Current.CancellationToken);

        Assert.Equal(6, progress.Receipts.Length);
        var directory = Assert.IsType<RouteMoveDirectoryCreationReceipt>(progress.Receipts[0]);
        Assert.Equal(FilesystemEffectState.Applied, directory.Receipt.EffectState);
        Assert.Equal(FilesystemVerificationState.Verified, directory.Receipt.VerificationState);
        var destination = Assert.IsType<RouteMoveFileChangeReceipt>(progress.Receipts[1]);
        Assert.Equal(FilesystemEffectState.Applied, destination.Receipt.EffectState);
        Assert.Equal(FilesystemVerificationState.Verified, destination.Receipt.VerificationState);
        var failed = Assert.IsType<RouteMoveFileChangeReceipt>(progress.Receipts[2]);
        Assert.Equal(FilesystemEffectState.NotStarted, failed.Receipt.EffectState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, failed.Receipt.NotStartedReason);
        AssertAllLaterNotStarted(progress.Receipts.Skip(3));
        Assert.Contains(
            progress.Findings,
            finding => finding.Code == RouteMoveFindingCode.TargetChangedDuringApply);
        Assert.Equal(RouteMoveRecoveryState.Retained, progress.Recovery.State);
        Assert.Equal(RouteMoveVerificationState.Unknown, progress.Verification);
        Assert.True(Directory.Exists(workspace.Absolute(".agents/archive/application")));
        Assert.Equal(
            sourceBefore,
            workspace.ReadText(RouteMoveIntegrationWorkspace.ApplicationCategoryDestination));
        Assert.Equal(mutatedArchive, workspace.ReadText(".agents/archive/_archive.md"));
        Assert.Equal(sourceBefore, workspace.ReadText(RouteMoveIntegrationWorkspace.ApplicationCategoryPath));
        Assert.Equal(lifecycleBefore, workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath));
    }

    [Fact(DisplayName = "Route Move cancellation leaves every heterogeneous effect not started")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task CancellationLeavesEveryEffectNotStarted()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-ordered-cancellation");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);
        var before = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var progress = await RouteMoveIntegrationWorkspace.CreateEffectApplication()
            .ApplyAsync(
                new RouteMoveEffectApplicationInput
                {
                    Plan = plan,
                    Lease = lease,
                    RecoveryPreparation = prepared,
                },
                cancellation.Token);

        Assert.Equal(6, progress.Receipts.Length);
        AssertAllNotStarted(progress.Receipts, FilesystemNotStartedReason.Cancelled);
        Assert.Contains(
            progress.Findings,
            finding => finding.Code == RouteMoveFindingCode.Interrupted);
        Assert.Equal(RouteMoveRecoveryState.Retained, progress.Recovery.State);
        Assert.Equal(RouteMoveVerificationState.NotRequested, progress.Verification);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(workspace.Absolute(".agents/archive/application")));
    }

    [Fact(DisplayName = "Route Move post-verification recovery deletion cancelled before candidate selection reports interrupted unknown recovery without inferred retention")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task PostVerificationRecoveryDeletionCancelledBeforeCandidateSelectionReportsInterruptedUnknownWithoutInferredRetention()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-verified-retained-cleanup");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var lifecycleBefore = workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.False(Directory.Exists(workspace.Absolute(".agents/archive/application")));

        var progress = await RouteMoveIntegrationWorkspace.CreateEffectApplication()
            .ApplyAsync(
                new RouteMoveEffectApplicationInput
                {
                    Plan = plan,
                    Lease = lease,
                    RecoveryPreparation = prepared,
                },
                TestContext.Current.CancellationToken);
        var verification = await RouteMoveIntegrationWorkspace.CreateAppliedVerifier()
            .VerifyAsync(
                new RouteMoveAppliedVerificationInput
                {
                    Plan = plan,
                    Lease = lease,
                    Progress = progress,
                },
                TestContext.Current.CancellationToken);

        AssertAllVerified(progress.Receipts);
        Assert.True(
            verification.State == RouteMoveAppliedVerificationState.Verified,
            verification.Cause);
        Assert.Null(verification.Cause);
        Assert.Equal(lifecycleBefore, workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath));
        Assert.True(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.ApplicationCategoryDestination)));
        Assert.False(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.ApplicationCategoryPath)));
        Assert.False(Directory.Exists(workspace.Absolute(".agents/guidance/application")));

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var deletion = await RouteMoveRecoveryLifecycle.DeleteExactAsync(
            new RouteMoveRecoveryDeletionInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
                Preparation = preparation,
            },
            cancellation.Token);

        Assert.Equal(RouteMoveRecoveryState.Unknown, deletion.Recovery.State);
        Assert.Null(deletion.Recovery.ResidualPath);
        Assert.Equal(RouteMoveFindingCode.Interrupted, deletion.Finding?.Code);
        Assert.Equal(CliSemanticStatus.Interrupted, deletion.Finding?.Status);
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.Equal(lifecycleBefore, workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath));
    }

    [Fact(DisplayName = "Route Move final verification rejects lifecycle drift after applied effects")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task FinalVerificationRejectsChangedLifecycleObservation()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-final-lifecycle-drift");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);
        var progress = await RouteMoveIntegrationWorkspace.CreateEffectApplication()
            .ApplyAsync(
                new RouteMoveEffectApplicationInput
                {
                    Plan = plan,
                    Lease = lease,
                    RecoveryPreparation = prepared,
                },
                TestContext.Current.CancellationToken);
        AssertAllVerified(progress.Receipts);
        workspace.SeedScenario("unrelated-ownership-claim");

        var verification = await RouteMoveIntegrationWorkspace.CreateAppliedVerifier()
            .VerifyAsync(
                new RouteMoveAppliedVerificationInput
                {
                    Plan = plan,
                    Lease = lease,
                    Progress = progress,
                },
                TestContext.Current.CancellationToken);

        Assert.Equal(RouteMoveAppliedVerificationState.Failed, verification.State);
        Assert.False(string.IsNullOrWhiteSpace(verification.Cause));
        Assert.True(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.ApplicationCategoryDestination)));
        Assert.False(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.ApplicationCategoryPath)));
        Assert.True(File.Exists(preparation.BundlePath));
    }

    [Fact(DisplayName = "Route Move final verification rejects an ambiguous destination topology")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task FinalVerificationRejectsAmbiguousDestinationTopology()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-final-destination-ambiguity");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);
        var progress = await RouteMoveIntegrationWorkspace.CreateEffectApplication()
            .ApplyAsync(
                new RouteMoveEffectApplicationInput
                {
                    Plan = plan,
                    Lease = lease,
                    RecoveryPreparation = prepared,
                },
                TestContext.Current.CancellationToken);
        AssertAllVerified(progress.Receipts);

        const string ambiguousEntrypoint = ".agents/archive/application/index.md";
        var ambiguousEntrypointPath = workspace.Absolute(ambiguousEntrypoint);
        File.WriteAllText(
            ambiguousEntrypointPath,
            workspace.ReadText(RouteMoveIntegrationWorkspace.ApplicationCategoryDestination));
        try
        {
            var verification = await RouteMoveIntegrationWorkspace.CreateAppliedVerifier()
                .VerifyAsync(
                    new RouteMoveAppliedVerificationInput
                    {
                        Plan = plan,
                        Lease = lease,
                        Progress = progress,
                    },
                    TestContext.Current.CancellationToken);

            Assert.Equal(RouteMoveAppliedVerificationState.Failed, verification.State);
            Assert.False(string.IsNullOrWhiteSpace(verification.Cause));
            Assert.Equal(
                plan.Projection.FileChanges[0].IntendedBytes,
                File.ReadAllBytes(workspace.Absolute(RouteMoveIntegrationWorkspace.ApplicationCategoryDestination)));
            Assert.False(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.ApplicationCategoryPath)));
            Assert.True(File.Exists(preparation.BundlePath));
        }
        finally
        {
            File.Delete(ambiguousEntrypointPath);
        }
    }

    [Fact(DisplayName = "Route Move changed recovery identity is a failed unknown disposition")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task ChangedRecoveryIdentityFailsUnknownWithoutDeletingReplacement()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-recovery-identity-changed");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.ReplaceRecoveryWithDirectory(preparation);
        var lifecycleBefore = workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath);

        var deletion = await RouteMoveRecoveryLifecycle.DeleteExactAsync(
            new RouteMoveRecoveryDeletionInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
                Preparation = preparation,
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteMoveRecoveryState.Unknown, deletion.Recovery.State);
        Assert.Null(deletion.Recovery.ResidualPath);
        Assert.Equal(RouteMoveFindingCode.RecoveryFailed, deletion.Finding?.Code);
        Assert.Equal(CliSemanticStatus.Failed, deletion.Finding?.Status);
        Assert.True(Directory.Exists(preparation.BundlePath));
        Assert.Equal(lifecycleBefore, workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath));
    }

    private static void AssertAllLaterNotStarted(IEnumerable<RouteMoveApplicationReceipt> receipts)
    {
        foreach (var receipt in receipts)
        {
            Assert.Equal(FilesystemEffectState.NotStarted, ReadEffectState(receipt));
            Assert.Equal(FilesystemNotStartedReason.ApplicationFailed, ReadNotStartedReason(receipt));
        }
    }

    private static void AssertAllNotStarted(
        IEnumerable<RouteMoveApplicationReceipt> receipts,
        FilesystemNotStartedReason reason)
    {
        foreach (var receipt in receipts)
        {
            Assert.Equal(FilesystemEffectState.NotStarted, ReadEffectState(receipt));
            Assert.Equal(reason, ReadNotStartedReason(receipt));
        }
    }

    private static void AssertAllVerified(IEnumerable<RouteMoveApplicationReceipt> receipts)
    {
        foreach (var receipt in receipts)
        {
            Assert.Equal(FilesystemEffectState.Applied, ReadEffectState(receipt));
            Assert.Equal(FilesystemVerificationState.Verified, ReadVerificationState(receipt));
        }
    }

    private static FilesystemEffectState ReadEffectState(RouteMoveApplicationReceipt receipt)
        => receipt switch
        {
            RouteMoveDirectoryCreationReceipt creation => creation.Receipt.EffectState,
            RouteMoveFileChangeReceipt change => change.Receipt.EffectState,
            RouteMoveDirectoryDeletionReceipt deletion => deletion.Receipt.State.EffectState,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt, "Unknown Route Move receipt."),
        };

    private static FilesystemVerificationState ReadVerificationState(
        RouteMoveApplicationReceipt receipt)
        => receipt switch
        {
            RouteMoveDirectoryCreationReceipt creation => creation.Receipt.VerificationState,
            RouteMoveFileChangeReceipt change => change.Receipt.VerificationState,
            RouteMoveDirectoryDeletionReceipt deletion => deletion.Receipt.State.VerificationState,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt, "Unknown Route Move receipt."),
        };

    private static FilesystemNotStartedReason? ReadNotStartedReason(
        RouteMoveApplicationReceipt receipt)
        => receipt switch
        {
            RouteMoveDirectoryCreationReceipt creation => creation.Receipt.NotStartedReason,
            RouteMoveFileChangeReceipt change => change.Receipt.NotStartedReason,
            RouteMoveDirectoryDeletionReceipt deletion => deletion.Receipt.State.NotStartedReason,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt, "Unknown Route Move receipt."),
        };
}
