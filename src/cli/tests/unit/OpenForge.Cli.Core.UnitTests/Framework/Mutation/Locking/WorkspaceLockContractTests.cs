using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Framework.Mutation.Locking;

public sealed class WorkspaceLockContractTests
{
    [Fact(DisplayName = "Workspace lock requests retain one cohesive workspace command and operation identity"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void WorkspaceLockRequestRetainsCohesiveIdentity()
    {
        var workspace = Workspace();
        var operationId = Guid.NewGuid();

        var request = new WorkspaceLockRequest(
            workspace: workspace,
            command: "index",
            operationId: operationId);

        Assert.Same(workspace, request.Workspace);
        Assert.Equal("index", request.Command);
        Assert.Equal(operationId, request.OperationId);
        Assert.Equal(Path.Combine(workspace.LexicalRoot, WorkspaceLockRequest.RelativePath), request.LogicalPath);
    }

    [Fact(DisplayName = "Workspace lock requests reject missing identity and invalid operation state"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void WorkspaceLockRequestRejectsInvalidState()
    {
        var workspace = Workspace();

        Assert.Throws<ArgumentNullException>(() => new WorkspaceLockRequest(null, "index", Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() => new WorkspaceLockRequest(workspace, "", Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() => new WorkspaceLockRequest(workspace, "index", Guid.Empty));
    }

    [Fact(DisplayName = "Workspace lock results keep failure and cancellation distinct from ownership"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void WorkspaceLockResultsKeepNonOwnershipStatesDistinct()
    {
        var failure = new FilesystemFailure(FilesystemFailureKind.AccessDenied, "Access was denied.");
        var failed = WorkspaceLockResult.Failed(failure);
        var cancelled = WorkspaceLockResult.Cancelled();
        var failedAfterBootstrap = WorkspaceLockResult.Failed(
            failure,
            WorkspaceLockBootstrapOutcome.Existing);
        var cancelledAfterBootstrap = WorkspaceLockResult.Cancelled(
            WorkspaceLockBootstrapOutcome.Materialized);

        Assert.Equal(WorkspaceLockState.Failed, failed.State);
        Assert.Same(failure, failed.Failure);
        Assert.Null(failed.Lease);
        Assert.Null(failed.BootstrapOutcome);
        Assert.Equal(WorkspaceLockState.Cancelled, cancelled.State);
        Assert.Null(cancelled.Lease);
        Assert.Null(cancelled.Failure);
        Assert.Null(cancelled.BootstrapOutcome);
        Assert.Equal(
            WorkspaceLockBootstrapOutcome.Existing,
            failedAfterBootstrap.BootstrapOutcome);
        Assert.Equal(
            WorkspaceLockBootstrapOutcome.Materialized,
            cancelledAfterBootstrap.BootstrapOutcome);
        Assert.Throws<ArgumentNullException>(() => WorkspaceLockResult.Acquired(
            lease: null,
            bootstrapOutcome: WorkspaceLockBootstrapOutcome.Existing));
    }

    [Fact(DisplayName = "Workspace lock results require a defined bootstrap outcome for acquired ownership"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void WorkspaceLockAcquiredResultRequiresDefinedBootstrapOutcome()
    {
        using var temporary = TemporaryWorkspace.Create("lock-result-bootstrap-outcome");
        var lockPath = temporary.CreateFile(WorkspaceLockRequest.RelativePath, "lock");
        var workspace = new CliWorkspace(
            temporary.Path,
            temporary.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new WorkspaceLockRequest(workspace, "index", Guid.NewGuid());
        using var lease = new WorkspaceLockLease(
            request,
            logicalPath: request.LogicalPath,
            physicalPath: lockPath,
            handle: Open(lockPath));
        var result = WorkspaceLockResult.Acquired(
            lease,
            WorkspaceLockBootstrapOutcome.Existing);
        var undefined = (WorkspaceLockBootstrapOutcome)int.MaxValue;

        Assert.Equal(
            [WorkspaceLockBootstrapOutcome.Existing, WorkspaceLockBootstrapOutcome.Materialized],
            Enum.GetValues<WorkspaceLockBootstrapOutcome>());
        Assert.Equal(WorkspaceLockState.Acquired, result.State);
        Assert.Same(lease, result.Lease);
        Assert.Equal(WorkspaceLockBootstrapOutcome.Existing, result.BootstrapOutcome);
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspaceLockResult.Acquired(
            lease,
            undefined));
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspaceLockResult.Failed(
            new FilesystemFailure(FilesystemFailureKind.InputOutput, "Failed."),
            undefined));
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspaceLockResult.Cancelled(
            undefined));
    }

    [Fact(DisplayName = "Workspace lock leases reject forged logical, physical, and handle identities"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void WorkspaceLockLeaseRejectsForgedIdentity()
    {
        using var temporary = TemporaryWorkspace.Create("lock-lease-invariants");
        var lockPath = temporary.CreateFile(WorkspaceLockRequest.RelativePath, "lock");
        var otherPath = temporary.CreateFile("other.lock", "other");
        var workspace = new CliWorkspace(
            temporary.Path,
            temporary.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new WorkspaceLockRequest(workspace, "index", Guid.NewGuid());

        using (var handle = Open(lockPath))
        {
            Assert.Throws<ArgumentException>(() => new WorkspaceLockLease(
                request,
                logicalPath: otherPath,
                physicalPath: lockPath,
                handle: handle));
        }

        using (var handle = Open(lockPath))
        {
            Assert.Throws<ArgumentException>(() => new WorkspaceLockLease(
                request,
                logicalPath: request.LogicalPath,
                physicalPath: otherPath,
                handle: handle));
        }

        using (var handle = Open(lockPath))
        {
            Assert.Throws<ArgumentException>(() => new WorkspaceLockLease(
                request,
                logicalPath: request.LogicalPath,
                physicalPath: Path.Combine(Path.GetTempPath(), "open-forge-foreign-lock.lock"),
                handle: handle));
        }

        using (var handle = Open(otherPath))
        {
            Assert.Throws<ArgumentException>(() => new WorkspaceLockLease(
                request,
                logicalPath: request.LogicalPath,
                physicalPath: lockPath,
                handle: handle));
        }

        using (var handle = new FileStream(
                   lockPath,
                   new FileStreamOptions
                   {
                       Mode = FileMode.Open,
                       Access = FileAccess.Read,
                       Share = FileShare.Read,
                   }))
        {
            Assert.Throws<ArgumentException>(() => new WorkspaceLockLease(
                request,
                logicalPath: request.LogicalPath,
                physicalPath: lockPath,
                handle: handle));
        }
    }

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-lock-contract"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static FileStream Open(string path)
        => new(
            path,
            new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.ReadWrite,
                Share = FileShare.None,
            });
}
