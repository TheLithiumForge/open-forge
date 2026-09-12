using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking;

public sealed class WorkspaceLockManagerIntegrationTests
{
    [Fact(DisplayName = "Workspace lock owns one external exclusive OS handle and persists its zero-byte file")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task AcquireOwnsExternalHandleAndPersistsReusableFile()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-lock-ownership");
        using var lockStore = WorkspaceLockTestStore.Create("mutation-lock-store");
        var workspace = Workspace(temporary);
        var lockPath = lockStore.Track(workspace);
        var firstRequest = Request(workspace, "route create");

        var first = await lockStore.AcquireAsync(
            firstRequest,
            TestContext.Current.CancellationToken);
        var lease = Assert.IsType<WorkspaceLockLease>(first.Lease);
        Assert.Equal(WorkspaceLockState.Acquired, first.State);
        Assert.True(lease.IsHeldFor(workspace));
        Assert.Equal(lockPath, lease.LockPath);
        Assert.DoesNotContain(workspace.PhysicalRoot, lockPath, StringComparison.Ordinal);
        await Assert.ThrowsAsync<IOException>(async () =>
        {
            await using var ignored = new FileStream(
                lockPath,
                new FileStreamOptions
                {
                    Mode = FileMode.Open,
                    Access = FileAccess.ReadWrite,
                    Share = FileShare.ReadWrite,
                    Options = FileOptions.Asynchronous,
                });
        });

        var contended = await lockStore.AcquireAsync(
            Request(workspace, "index"),
            TestContext.Current.CancellationToken);

        Assert.Equal(WorkspaceLockState.Failed, contended.State);
        Assert.Equal(FilesystemFailureKind.InputOutput, contended.Failure?.Kind);
        Assert.Null(contended.Lease);

        await lease.DisposeAsync();
        Assert.False(lease.IsHeld);
        Assert.True(File.Exists(lockPath));
        Assert.Equal(0, new FileInfo(lockPath).Length);

        var reused = await lockStore.AcquireAsync(
            Request(workspace, "extension install"),
            TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceLockState.Acquired, reused.State);
        await Assert.IsType<WorkspaceLockLease>(reused.Lease).DisposeAsync();
        Assert.True(File.Exists(lockPath));
        Assert.False(Directory.Exists(temporary.Combine(".agents")));
    }

    [Fact(DisplayName = "Workspace lock cancellation before acquisition creates no external lock infrastructure")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task AcquireCancelledBeforeStoreCreationCreatesNothing()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-lock-cancelled");
        using var lockStore = WorkspaceLockTestStore.Create("mutation-lock-cancelled-store");
        var workspace = Workspace(temporary);
        var lockPath = lockStore.Track(workspace);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await lockStore.AcquireAsync(
            Request(workspace, "install"),
            cancellation.Token);

        Assert.Equal(WorkspaceLockState.Cancelled, result.State);
        Assert.Null(result.Lease);
        Assert.False(File.Exists(lockPath));
        Assert.False(Directory.Exists(WorkspaceLockPathIdentity.StoreDirectory(lockStore.StoreRoot)));
        Assert.False(Directory.Exists(temporary.Combine(".agents")));
    }

    [Fact(DisplayName = "Workspace lock rejects and preserves nonzero persistent content")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task AcquireRejectsAndPreservesNonzeroFile()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-lock-nonzero");
        using var lockStore = WorkspaceLockTestStore.Create("mutation-lock-nonzero-store");
        var workspace = Workspace(temporary);
        var lockPath = lockStore.Track(workspace);
        Directory.CreateDirectory(Path.GetDirectoryName(lockPath)
            ?? throw new InvalidOperationException("The external lock path requires a directory."));
        await File.WriteAllTextAsync(
            lockPath,
            "foreign-content",
            TestContext.Current.CancellationToken);

        var result = await lockStore.AcquireAsync(
            Request(workspace, "install"),
            TestContext.Current.CancellationToken);

        Assert.Equal(WorkspaceLockState.Failed, result.State);
        Assert.Equal(FilesystemFailureKind.InvalidPath, result.Failure?.Kind);
        Assert.Equal(
            "foreign-content",
            await File.ReadAllTextAsync(lockPath, TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Workspace lock rejects directory and symbolic-link targets without following them")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task AcquireRejectsNonordinaryExternalTargets()
    {
        using var outside = TemporaryWorkspace.Create("mutation-lock-outside");
        var outsideFile = outside.CreateFile("outside.lock", "outside");
        using var directoryWorkspace = TemporaryWorkspace.Create("mutation-lock-directory-target");
        using var aliasWorkspace = TemporaryWorkspace.Create("mutation-lock-alias-target");
        using var lockStore = WorkspaceLockTestStore.Create("mutation-lock-unsafe-store");
        var directorySubject = Workspace(directoryWorkspace);
        var aliasSubject = Workspace(aliasWorkspace);
        var directoryPath = lockStore.Track(directorySubject);
        var aliasPath = lockStore.Track(aliasSubject);
        Directory.CreateDirectory(directoryPath);
        Directory.CreateDirectory(Path.GetDirectoryName(aliasPath)
            ?? throw new InvalidOperationException("The external lock path requires a directory."));
        File.CreateSymbolicLink(aliasPath, outsideFile);

        var directoryResult = await lockStore.AcquireAsync(
            Request(directorySubject, "index"),
            TestContext.Current.CancellationToken);
        var aliasResult = await lockStore.AcquireAsync(
            Request(aliasSubject, "index"),
            TestContext.Current.CancellationToken);

        Assert.Equal(WorkspaceLockState.Failed, directoryResult.State);
        Assert.Equal(FilesystemFailureKind.InvalidPath, directoryResult.Failure?.Kind);
        Assert.Equal(WorkspaceLockState.Failed, aliasResult.State);
        Assert.Equal(FilesystemFailureKind.InvalidPath, aliasResult.Failure?.Kind);
        Assert.Equal(
            "outside",
            await File.ReadAllTextAsync(outsideFile, TestContext.Current.CancellationToken));
    }

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static WorkspaceLockRequest Request(
        CliWorkspace workspace,
        string command)
        => new(workspace, command, Guid.NewGuid());
}
