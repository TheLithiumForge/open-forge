using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Models.Mutation;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallLockFailureIntegrationTests
{
    [Fact(DisplayName = "Install preserves the busy lock reason and local sentinel path in its finding")]
    [Trait("Feature", "install-command"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task HeldWorkspaceLockReportsTypedFailureAndPath()
    {
        using var workspace = InstallOperationWorkspace.Create("install-lock-failure");
        var lockResult = await new WorkspaceLockManager(workspace.LockStoreRoot).AcquireAsync(
            new WorkspaceLockRequest(workspace.Workspace, InstallDefinitions.CommandIdentity, Guid.NewGuid()),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);

        var result = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);

        var finding = Assert.Single(result.Findings, finding => finding.Code == InstallFindingCode.WorkspaceUnsafe);
        var failure = Assert.IsType<WorkspaceLockFailureInfo>(finding.LockFailure);
        Assert.Equal(WorkspaceLockFailureReason.Busy, failure.Reason);
        Assert.Equal(TestDataHome.LocalWorkspaceLockPath(workspace.PhysicalPath), failure.Path);
    }
}
