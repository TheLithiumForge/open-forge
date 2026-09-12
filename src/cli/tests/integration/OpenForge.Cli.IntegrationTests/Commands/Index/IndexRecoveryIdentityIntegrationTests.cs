using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Shared.Operation;
using OpenForge.Cli.Core.Commands.Index.Shared.Planning;
using OpenForge.Cli.Core.Commands.Index.Shared.Projection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Recovery;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

public sealed class IndexRecoveryIdentityIntegrationTests
{
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "index"), Trait("Evidence", "Integration")]
    public async Task CleanupRequiresThePreparedAttribution(bool changed)
    {
        using var workspace = IndexOperationWorkspace.Create("index-recovery-identity");
        var request = workspace.Request(IndexMode.Apply);
        var resolver = new PhysicalPathResolver();
        var read = await new IndexProjectionReader(resolver).ReadAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(IndexProjectionReadState.Projected, read.State);
        Assert.NotNull(read.Projection);
        var plan = new IndexPlanBuilder().Build(new IndexPlanningInput { Request = request, Projection = read.Projection });
        var operationId = Guid.NewGuid();
        var application = new IndexApplicationContext(plan, operationId);
        var lockResult = await new WorkspaceLockManager(workspace.LockStoreRoot).AcquireAsync(
            new WorkspaceLockRequest(workspace.Workspace, IndexDefinitions.CommandIdentity, operationId),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);
        var validation = await new MutationRevalidator(new FileExpectationValidator(resolver)).ValidateAsync(
            lease, plan.Updates, TestContext.Current.CancellationToken);
        Assert.Equal(MutationValidationState.Valid, validation.State);
        var prepared = await IndexRecoveryLifecycle.PrepareAsync(application, TestContext.Current.CancellationToken);
        Assert.True(prepared.CanApply);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            var preparedApplication = new IndexPreparedApplication(
                application: application, lease: lease, validation: validation, preparation: preparation);
            var workspaceBefore = workspace.SnapshotHashes();
            var observed = await RecoveryFinalIdentityFixture.ObserveAsync(workspace.Workspace, preparation);
            var changedAttribution = RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Repair, RecoveryBundleOperation.Repair, workspace.Workspace);
            Assert.NotEqual(preparation.Attribution, changedAttribution);
            if (changed)
            {
                await observed.ChangeAttributionAsync(changedAttribution);
            }

            var bundleBytes = await observed.AssertAdmissionAsync(
                preparation.Command,
                changed ? changedAttribution : preparation.Attribution);
            Assert.Equal(workspaceBefore, workspace.SnapshotHashes());

            var result = await IndexRecoveryLifecycle.DeleteAsync(preparedApplication, TestContext.Current.CancellationToken);

            await observed.AssertTargetsUnchangedAsync();
            Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
            if (changed)
            {
                Assert.True(File.Exists(preparation.BundlePath),
                    $"Cleanup deleted the changed valid final; observed state: {result.Recovery.State}.");
                Assert.Equal(bundleBytes, await File.ReadAllBytesAsync(preparation.BundlePath, TestContext.Current.CancellationToken));
                Assert.Equal(IndexRecoveryState.Unknown, result.Recovery.State);
                Assert.Equal(IndexFindingCode.RecoveryFailed, result.FindingCode);
                Assert.Equal(preparation.BundlePath, result.Recovery.ResidualPath);
            }
            else
            {
                Assert.False(File.Exists(preparation.BundlePath));
                Assert.Equal(IndexRecoveryState.Removed, result.Recovery.State);
                Assert.Null(result.FindingCode);
                Assert.Null(result.Recovery.ResidualPath);
            }
        }
        finally
        {
            File.Delete(preparation.BundlePath);
        }
    }
}
