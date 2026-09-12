using OpenForge.Cli.Core.Commands.Library.Detach.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Application;
using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Completion;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Detach;

public sealed class LibraryDetachApplicationIntegrationTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("verified"), InlineData("no-recovery"), InlineData("changed-record"), InlineData("changed-leaf"), InlineData("cancelled")]
    public static async Task HeldLeaseRevalidatesWholePlanAndRetainsRealExecutionReceipts(string scenario)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var locks = WorkspaceLockTestStore.Create("library-detach-application");
        workspace.Source();
        workspace.Directory(".agents/directives");
        LibraryMutationApplicationData.Lifecycle(workspace);
        workspace.Record(LibraryMutationWorkspace.Leaf);
        workspace.Link();
        var plan = Plan(workspace);
        var recordChange = Assert.IsType<PlannedFileChange>(plan.RecordChange);
        var recordBefore = Assert.IsType<FileStateSnapshot>(plan.Input.Record.Snapshot);
        var link = Assert.Single(plan.Links);
        var recovery = RecoveryBundleInput.Create(workspace.Workspace, command: "library detach",
            attribution: RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Detach, workspace.Workspace),
            operationId: Guid.NewGuid(), targets:
            [
                RecoveryBundleTarget.Create(link, LibraryMutationApplicationData.Leaf(workspace, linked: true)),
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
                Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md",
                    new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);
                File.Delete(workspace.Absolute(LibraryMutationWorkspace.Leaf));
                using var replacement = new FileStream(workspace.Absolute(LibraryMutationWorkspace.Leaf), FileMode.CreateNew, FileAccess.Write);
                replacement.Write("Changed local leaf occupant."u8);
            }
            var before = workspace.Snapshot();
            using var cancellation = new CancellationTokenSource();
            if (scenario == "cancelled")
            {
                await cancellation.CancelAsync();
            }

            var execution = await LibraryDetachApplication.ApplyAsync(new LibraryDetachApplicationInput
            { Lease = lease, Plan = plan, RecoveryPreparation = preparation }, cancellation.Token);
            Assert.Equal(LibraryMutationWorkspace.SourceBytes,
                File.ReadAllText(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{LibraryMutationWorkspace.Leaf}")));
            if (scenario != "verified")
            {
                Assert.Equal(before, workspace.Snapshot());
                Assert.DoesNotContain(execution.Links, receipt => receipt.EffectState != FilesystemEffectState.NotStarted);
                Assert.True(execution.Record is null || execution.Record.EffectState == FilesystemEffectState.NotStarted);
                if (preparation is not null)
                {
                    Assert.True(File.Exists(preparation.BundlePath));
                }
                return;
            }

            Assert.Equal(FilesystemVerificationState.Verified, Assert.Single(execution.Links).VerificationState);
            Assert.Equal(FilesystemVerificationState.Verified, Assert.IsType<FileChangeReceipt>(execution.Record).VerificationState);
            Assert.Same(preparation, execution.RecoveryPreparation);
            Assert.Equal(LibraryRecordPublicationOrder.Last, execution.RecordPublicationOrder);
            Assert.True(Assert.IsType<LibrarySourceEffectScopeFacts>(execution.SourceEffectScope).IsComplete);
            Assert.Null(new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);
            Assert.False(File.Exists(workspace.Absolute(LibraryMutationWorkspace.RecordPath)));
            var retained = Assert.IsType<RecoveryBundlePreparation>(preparation);
            var read = await RecoveryBundleReader.ReadFinalAsync(workspace.Workspace, retained.BundlePath, TestContext.Current.CancellationToken);
            var candidate = RecoveryBundleCandidateSnapshot.VerifiedFinal(Assert.IsType<RecoveryBundleVerifiedRead>(read.Verified));
            var cleanup = await RecoveryBundleDeletionGuard.DeleteAsync(lease, candidate, TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundleDisposition.Removed, cleanup.Disposition);
            var result = LibraryDetachCompletion.Complete(new LibraryDetachCompletionInput
            {
                Request = plan.Input.Request,
                Plan = plan,
                Observations = plan.Input,
                Execution = execution with { RecoveryCleanup = cleanup },
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
            var replacementPath = workspace.Absolute(LibraryMutationWorkspace.Leaf);
            if (scenario == "changed-leaf" && File.Exists(replacementPath)
                && (File.GetAttributes(replacementPath) & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) == 0
                && File.ReadAllText(replacementPath) == "Changed local leaf occupant.")
            {
                File.Delete(replacementPath);
            }
            if (new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget == "../../shared/team-knowledge/.agents/directives/review.md")
            {
                File.Delete(workspace.Absolute(LibraryMutationWorkspace.Leaf));
            }

        }
    }

    private static LibraryDetachPlan Plan(LibraryMutationWorkspace workspace)
    {
        var record = LibraryMutationApplicationData.ReadRecord(workspace, exists: true, registered: true);
        var before = Assert.IsType<FileStateSnapshot>(record.Snapshot);
        var input = new LibraryDetachPlanningInput
        {
            Request = workspace.Detach(LibraryMode.Apply),
            Record = record,
            Mappings = LibraryMutationApplicationData.Mapping(workspace, linked: true),
            ConsumerBoundary = LibraryMutationApplicationData.Boundary(workspace),
            Ownership = LibraryMutationApplicationData.Ownership(workspace),
            GeneratedRegionChanges = [],
        };

        return new LibraryDetachPlan
        {
            Permissions = null,
            Input = input,
            State = LibraryPlanState.Complete,
            Directories = [],
            Links = [LibraryMutationApplicationData.Link(delete: true)],
            GeneratedRegions = [],
            RecordChange = PlannedFileChange.Delete(before.Expectation),
            IntendedRecord = null,
            Findings = [],
        };
    }
}
