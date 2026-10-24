using OpenForge.Cli.TestSupport.Isolation;
using OpenForge.Cli.Core.Commands.Shared.Models.Mutation;
using OpenForge.Cli.Core.Commands.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Storage;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.UnitTests.Framework.Mutation.Locking;

public sealed class WorkspaceRuntimeContractTests
{
    private const string RootName = "physical";
    private const string AliasName = "alias";
    private const string ChildName = "child";
    private const string LeafName = "file";
    private const string SiblingSuffix = "-other";
    private const string InvalidPath = "path";

    [Fact(DisplayName = "Runtime paths use physical identity and component boundary containment")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void RuntimePathsUsePhysicalIdentity()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), RootName));
        var workspace = new CliWorkspace(Path.Combine(root, AliasName), root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var runtime = TestDataHome.LocalRuntimeDirectory(root);
        Assert.Equal(runtime, WorkspaceRuntimePaths.DirectoryPath(workspace));
        Assert.Equal(TestDataHome.LocalWorkspaceLockPath(root), WorkspaceRuntimePaths.LockPath(workspace));
        Assert.Equal(TestDataHome.LocalRuntimeIgnorePath(root), WorkspaceRuntimePaths.IgnorePath(workspace));
        Assert.True(WorkspaceRuntimePaths.Contains(workspace, runtime));
        Assert.True(WorkspaceRuntimePaths.Contains(workspace, Path.Combine(runtime, ChildName, "..", LeafName)));
        Assert.False(WorkspaceRuntimePaths.Contains(workspace, runtime + SiblingSuffix));
        Assert.False(WorkspaceRuntimePaths.Contains(workspace, Path.Combine(runtime, "..", LeafName)));
        Assert.Equal(OperatingSystem.IsWindows(), WorkspaceRuntimePaths.Contains(workspace, runtime.ToUpperInvariant()));
        var location = RecoveryStoreLocator.Local(workspace);
        Assert.Equal(RecoveryStoreKind.Workspace, location.Kind);
        Assert.Equal(TestDataHome.LocalRecoveryStoreRoot(root), location.RootPath);
        Assert.Equal(TestDataHome.LocalRecoveryWorkspaceDirectory(root), location.WorkspaceDirectory);
        Assert.Null(location.LegacyLockStoreRoot);
    }

    [Theory(DisplayName = "Operations lock failures preserve every typed cause and the complete path")]
    [InlineData((int)WorkspaceLockFailureKind.Busy, (int)WorkspaceLockFailureReason.Busy)]
    [InlineData((int)WorkspaceLockFailureKind.AccessDenied, (int)WorkspaceLockFailureReason.AccessDenied)]
    [InlineData((int)WorkspaceLockFailureKind.Unsupported, (int)WorkspaceLockFailureReason.Unsupported)]
    [InlineData((int)WorkspaceLockFailureKind.InvalidTarget, (int)WorkspaceLockFailureReason.InvalidTarget)]
    [InlineData((int)WorkspaceLockFailureKind.IoError, (int)WorkspaceLockFailureReason.IoError)]
    [InlineData((int)WorkspaceLockFailureKind.Cancelled, (int)WorkspaceLockFailureReason.Cancelled)]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void FailureProjectionRetainsCauseAndPath(int kindValue, int reasonValue)
    {
        var kind = (WorkspaceLockFailureKind)kindValue;
        var reason = (WorkspaceLockFailureReason)reasonValue;
        var path = Path.Combine(Path.GetTempPath(), new string('a', 400));
        var failure = new WorkspaceLockFailure(kind, path);
        var projection = WorkspaceLockFailureProjection.From(failure);
        Assert.Equal(reason, projection.Reason);
        Assert.Equal(path, projection.Path);
        var result = WorkspaceLockResult.Failed(failure);
        Assert.Same(failure, result.LockFailure);
        Assert.NotNull(result.Failure);
        Assert.Equal(kind == WorkspaceLockFailureKind.Cancelled ? WorkspaceLockState.Cancelled : WorkspaceLockState.Failed, result.State);
    }

    [Fact(DisplayName = "Lock projections reject undefined causes and legacy results require live authority")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void InvalidLockStatesAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspaceLockFailureProjection.From(new((WorkspaceLockFailureKind)99, InvalidPath)));
        Assert.Throws<ArgumentException>(() => new LegacyWorkspaceLockResult(LegacyWorkspaceLockState.HeldReadOnly));
        Assert.Throws<ArgumentException>(() => new LegacyWorkspaceLockResult(LegacyWorkspaceLockState.Failed));
        Assert.Throws<ArgumentOutOfRangeException>(() => new LegacyWorkspaceLockResult((LegacyWorkspaceLockState)99));
    }
}
