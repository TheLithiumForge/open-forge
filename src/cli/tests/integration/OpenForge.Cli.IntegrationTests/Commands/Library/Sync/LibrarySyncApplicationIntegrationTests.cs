using System.Text;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Sync.Shared.Application;
using OpenForge.Cli.Core.Commands.Library.Sync.Shared.Completion;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Sync;

public sealed class LibrarySyncApplicationIntegrationTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("verified"), InlineData("no-recovery"), InlineData("changed-record"), InlineData("changed-leaf"), InlineData("cancelled")]
    public static async Task HeldLeaseRevalidatesWholePlanAndRetainsRealExecutionReceipts(string scenario)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var locks = WorkspaceLockTestStore.Create("library-sync-application");
        workspace.Source();
        workspace.Directory(".agents/directives");
        LibraryMutationApplicationData.Lifecycle(workspace);
        workspace.Record();
        var plan = Plan(workspace);
        var recordChange = Assert.IsType<PlannedFileChange>(plan.RecordChange);
        var recordBefore = Assert.IsType<FileStateSnapshot>(plan.Input.Record.Snapshot);
        var link = Assert.Single(plan.Links);
        var recovery = RecoveryBundleInput.Create(workspace.Workspace, command: "library sync",
            attribution: RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Sync, workspace.Workspace),
            operationId: Guid.NewGuid(), targets:
            [
                RecoveryBundleTarget.Create(link, LibraryMutationApplicationData.Leaf(workspace, linked: false)),
                RecoveryBundleTarget.Create(recordChange, recordBefore),
            ]);
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new WorkspaceLockRequest(workspace.Workspace, recovery.Command, recovery.OperationId), TestContext.Current.CancellationToken)).Lease);
        RecoveryBundlePreparation? preparation = null;
        try
        {
            if (scenario != "no-recovery")
            {
                var prepared = await RecoveryBundleStore.PrepareAsync(recovery, TestContext.Current.CancellationToken);
                Assert.True(prepared.State == RecoveryBundlePreparationState.Prepared,
                    $"Required real preparation failed before Library application assertion: {prepared.State}; {prepared.Cause}");
                preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
            }

            if (scenario == "changed-record")
            {
                workspace.Replace(LibraryMutationWorkspace.RecordPath, "Changed local record occupant.");
            }
            if (scenario == "changed-leaf")
            {

                workspace.Write(LibraryMutationWorkspace.Leaf, "Changed local leaf occupant.");
            }
            var before = workspace.Snapshot();
            using var cancellation = new CancellationTokenSource();
            if (scenario == "cancelled")
            {
                await cancellation.CancelAsync();
            }

            var outcome = await LibrarySyncApplication.ApplyAsync(new LibrarySyncApplicationInput
            { Lease = lease, Plan = plan, RecoveryPreparation = preparation }, cancellation.Token);
            Assert.Equal(outcome.Execution.Links, outcome.Links);
            Assert.Same(outcome.Execution.Record, outcome.Record);
            Assert.Equal(LibraryMutationWorkspace.SourceBytes,
                File.ReadAllText(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{LibraryMutationWorkspace.Leaf}")));
            if (scenario != "verified")
            {
                Assert.Equal(before, workspace.Snapshot());
                Assert.DoesNotContain(outcome.Links, receipt => receipt.EffectState != FilesystemEffectState.NotStarted);
                Assert.True(outcome.Record is null || outcome.Record.EffectState == FilesystemEffectState.NotStarted);
                if (preparation is not null)
                {
                    Assert.True(File.Exists(preparation.BundlePath));
                }
                return;
            }

            Assert.Equal(FilesystemVerificationState.Verified, Assert.Single(outcome.Links).VerificationState);
            Assert.Equal(FilesystemVerificationState.Verified, Assert.IsType<FileChangeReceipt>(outcome.Record).VerificationState);
            Assert.Same(preparation, outcome.Execution.RecoveryPreparation);
            Assert.Equal(LibraryRecordPublicationOrder.Last, outcome.Execution.RecordPublicationOrder);
            Assert.True(Assert.IsType<LibrarySourceEffectScopeFacts>(outcome.Execution.SourceEffectScope).IsComplete);
            Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md", new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);
            Assert.Equal(recordChange.IntendedBytes.ToArray(), File.ReadAllBytes(workspace.Absolute(LibraryMutationWorkspace.RecordPath)));
            var retained = Assert.IsType<RecoveryBundlePreparation>(preparation);
            var read = await RecoveryBundleReader.ReadFinalAsync(workspace.Workspace, retained.BundlePath, TestContext.Current.CancellationToken);
            var candidate = RecoveryBundleCandidateSnapshot.VerifiedFinal(Assert.IsType<RecoveryBundleVerifiedRead>(read.Verified));
            var cleanup = await RecoveryBundleDeletionGuard.DeleteAsync(lease, candidate, TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundleDisposition.Removed, cleanup.Disposition);
            var result = LibrarySyncCompletion.Complete(new LibrarySyncCompletionInput
            {
                Request = plan.Input.Request,
                Plan = plan,
                Observations = plan.Input,
                Execution = outcome.Execution with { RecoveryCleanup = cleanup },
            });
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(LibraryRecoveryState.Removed, result.Result.Application.Recovery.State);
            Assert.True(result.Result.Application.RecordPublication.PublishedLast);
        }
        finally
        {
            if (preparation is not null && File.Exists(preparation.BundlePath))
            {
                File.Delete(preparation.BundlePath);
            }
            if (new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget == "../../shared/team-knowledge/.agents/directives/review.md")
            {
                File.Delete(workspace.Absolute(LibraryMutationWorkspace.Leaf));
            }

        }
    }

    private static LibrarySyncPlan Plan(LibraryMutationWorkspace workspace)
    {
        var record = LibraryMutationApplicationData.ReadRecord(workspace, exists: true, registered: false);
        var before = Assert.IsType<FileStateSnapshot>(record.Snapshot);
        var input = new LibrarySyncPlanningInput
        {
            Request = workspace.Sync(LibraryMode.Apply),
            Record = record,
            Source = LibraryMutationApplicationData.Inventory(workspace),
            Mappings = LibraryMutationApplicationData.Mapping(workspace, linked: false),
            ConsumerBoundary = LibraryMutationApplicationData.Boundary(workspace),
            Ownership = LibraryMutationApplicationData.Ownership(workspace),
            GeneratedRegionChanges = [],
        };
        var intendedBytes = Encoding.UTF8.GetBytes("""
            {"schemaVersion":1,"libraries":[{"id":"team-knowledge","sourceRoot":"shared/team-knowledge","destinationRoot":".","paths":[".agents/directives/review.md"]}]}
            """);
        return new LibrarySyncPlan
        {
            Permissions = null,
            Input = input,
            State = LibraryPlanState.Complete,
            Directories = [],
            Links = [LibraryMutationApplicationData.Link(delete: false)],
            GeneratedRegions = [],
            RecordChange = PlannedFileChange.Replace(before.Expectation, intendedBytes),
            IntendedRecord = LibraryMutationApplicationData.Record(registered: true),
            Findings = [],
        };
    }
}
