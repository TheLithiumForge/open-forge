using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Mutation.Locking;

public sealed class WorkspaceLockContractTests
{
    [Trait("Boundary", "Processing")]
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
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace lock requests reject invalid command and operation state"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void WorkspaceLockRequestRejectsInvalidState()
    {
        var workspace = Workspace();

        Assert.Throws<ArgumentException>(() => new WorkspaceLockRequest(workspace, "", Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() => new WorkspaceLockRequest(workspace, "index", Guid.Empty));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace lock results keep failure and cancellation distinct from ownership"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void WorkspaceLockResultsKeepNonOwnershipStatesDistinct()
    {
        var failure = new FilesystemFailure(FilesystemFailureKind.AccessDenied, "Access was denied.");
        var failed = WorkspaceLockResult.Failed(failure);
        var cancelled = WorkspaceLockResult.Cancelled();

        Assert.Equal(WorkspaceLockState.Failed, failed.State);
        Assert.Same(failure, failed.Failure);
        Assert.Null(failed.Lease);
        Assert.Equal(WorkspaceLockState.Cancelled, cancelled.State);
        Assert.Null(cancelled.Lease);
        Assert.Null(cancelled.Failure);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Workspace lock filenames combine a bounded display prefix with the full authoritative key"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    [InlineData("open-forge", "open-forge")]
    [InlineData("Open Forge!", "open-forge")]
    [InlineData("---", WorkspaceLockPathIdentity.FallbackFriendlyName)]
    public void WorkspaceLockPathUsesFriendlyPrefixAndFullKey(
        string directoryName,
        string expectedFriendlyName)
    {
        var physicalRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), directoryName));
        var workspace = new CliWorkspace(
            lexicalRoot: physicalRoot,
            physicalRoot: physicalRoot,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var storeRoot = WorkspaceLockStoreRoot.FromAbsolutePath(
            Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-lock-store-contract")));
        var key = WorkspaceIdentity.Key(physicalRoot);

        var lockPath = WorkspaceLockPathIdentity.LockPath(storeRoot, workspace);

        Assert.Equal(64, key.Length);
        Assert.Equal(
            $"{expectedFriendlyName}-{key}.lock",
            Path.GetFileName(lockPath));
        Assert.Equal(
            WorkspaceLockPathIdentity.StoreDirectory(storeRoot),
            Path.GetDirectoryName(lockPath));
        Assert.DoesNotContain(physicalRoot, Path.GetFileName(lockPath), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace lock friendly names are bounded"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void WorkspaceLockFriendlyNameIsBounded()
    {
        var directoryName = new string('a', WorkspaceLockPathIdentity.MaximumFriendlyNameLength + 16);
        var physicalRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), directoryName));

        var friendlyName = WorkspaceLockPathIdentity.FriendlyName(physicalRoot);

        Assert.Equal(WorkspaceLockPathIdentity.MaximumFriendlyNameLength, friendlyName.Length);
        Assert.All(friendlyName, character => Assert.Equal('a', character));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Workspace identity normalizes trailing separators before hashing"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void WorkspaceIdentityNormalizesBeforeHashing()
    {
        var physicalRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-identity"));

        Assert.Equal(
            WorkspaceIdentity.Key(physicalRoot),
            WorkspaceIdentity.Key(physicalRoot + Path.DirectorySeparatorChar));
        Assert.Equal(
            WorkspaceIdentity.NormalizePhysicalPath(physicalRoot),
            WorkspaceIdentity.NormalizePhysicalPath(physicalRoot + Path.DirectorySeparatorChar));
    }

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-lock-contract"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
