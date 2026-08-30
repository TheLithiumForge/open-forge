using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Locking;

public sealed class WorkspaceLockManagerIntegrationTests
{
    [Fact(DisplayName = "Workspace lock owns one exclusive OS handle until disposal")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task AcquireOwnsExclusiveHandleAndPreservesContendedBytes()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-lock-ownership");
        var lockPath = temporary.CreateFile(WorkspaceLockRequest.RelativePath, "stale");
        var workspace = Workspace(temporary);
        var manager = new WorkspaceLockManager(new PhysicalPathResolver());
        var firstRequest = Request(workspace, "route create");

        var first = await manager.AcquireAsync(firstRequest, TestContext.Current.CancellationToken);
        var lease = Assert.IsType<WorkspaceLockLease>(first.Lease);
        Assert.Equal(WorkspaceLockState.Acquired, first.State);
        Assert.True(lease.IsHeld);
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

        var contended = await manager.AcquireAsync(
            Request(workspace, "index"),
            TestContext.Current.CancellationToken);

        Assert.Equal(WorkspaceLockState.Failed, contended.State);
        Assert.Equal(FilesystemFailureKind.InputOutput, contended.Failure?.Kind);
        Assert.Null(contended.Lease);

        await lease.DisposeAsync();
        Assert.False(lease.IsHeld);
        Assert.Equal(
            "stale"u8.ToArray(),
            await File.ReadAllBytesAsync(lockPath, TestContext.Current.CancellationToken));

        var reused = await manager.AcquireAsync(
            Request(workspace, "extension install"),
            TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceLockState.Acquired, reused.State);
        await Assert.IsType<WorkspaceLockLease>(reused.Lease).DisposeAsync();
        Assert.Equal(WorkspaceLockBootstrapOutcome.Existing, first.BootstrapOutcome);
        Assert.Equal(WorkspaceLockBootstrapOutcome.Existing, contended.BootstrapOutcome);
        Assert.Equal(WorkspaceLockBootstrapOutcome.Existing, reused.BootstrapOutcome);
    }

    [Fact(DisplayName = "Workspace lock bootstraps a missing contained lock directory")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task AcquireBootstrapsMissingContainedDirectory()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-lock-bootstrap");
        var workspace = Workspace(temporary);
        var request = Request(workspace, "install");
        var manager = new WorkspaceLockManager(new PhysicalPathResolver());

        var result = await manager.AcquireAsync(request, TestContext.Current.CancellationToken);

        var lease = Assert.IsType<WorkspaceLockLease>(result.Lease);
        Assert.Equal(WorkspaceLockState.Acquired, result.State);
        Assert.True(lease.IsHeld);
        await lease.DisposeAsync();
        Assert.Empty(await File.ReadAllBytesAsync(
            request.LogicalPath,
            TestContext.Current.CancellationToken));

        File.Delete(request.LogicalPath);
        var lockDirectory = Path.GetDirectoryName(request.LogicalPath)
            ?? throw new InvalidOperationException("The workspace lock path requires a directory.");
        Directory.Delete(lockDirectory);
        Assert.Equal(WorkspaceLockBootstrapOutcome.Materialized, result.BootstrapOutcome);
    }

    [Fact(DisplayName = "Workspace lock cancellation creates no lock artifacts")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task AcquireCancelledBeforeBootstrapCreatesNothing()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-lock-cancelled");
        var request = Request(Workspace(temporary), "install");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await new WorkspaceLockManager(new PhysicalPathResolver())
            .AcquireAsync(request, cancellation.Token);

        Assert.Equal(WorkspaceLockState.Cancelled, result.State);
        Assert.Null(result.BootstrapOutcome);
        Assert.False(Directory.Exists(Path.GetDirectoryName(request.LogicalPath)));
        Assert.False(File.Exists(request.LogicalPath));
    }

    [Fact(DisplayName = "Workspace lock blocks unsafe and non-directory containers")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task AcquireBlocksExternalAliasAndNonDirectoryContainer()
    {
        using var outside = TemporaryWorkspace.Create("mutation-lock-outside");
        var outsideLock = outside.CreateFile("outside.lock", "outside");
        using var aliasWorkspace = TemporaryWorkspace.Create("mutation-lock-alias");
        aliasWorkspace.CreateDirectorySymbolicLink(".agents", outside.Path);
        using var fileAliasWorkspace = TemporaryWorkspace.Create("mutation-lock-file-alias");
        fileAliasWorkspace.CreateDirectory(".agents");
        fileAliasWorkspace.CreateFileSymbolicLink(
            WorkspaceLockRequest.RelativePath,
            outsideLock);
        using var internalAliasWorkspace = TemporaryWorkspace.Create("mutation-lock-internal-alias");
        var internalTarget = internalAliasWorkspace.CreateFile("retained.txt", "retained");
        internalAliasWorkspace.CreateDirectorySymbolicLink(
            ".agents",
            internalAliasWorkspace.CreateDirectory("lock-container"));
        using var internalFileAliasWorkspace = TemporaryWorkspace.Create("mutation-lock-internal-file-alias");
        var internalFileTarget = internalFileAliasWorkspace.CreateFile("retained.txt", "retained");
        internalFileAliasWorkspace.CreateDirectory(".agents");
        internalFileAliasWorkspace.CreateFileSymbolicLink(
            WorkspaceLockRequest.RelativePath,
            internalFileTarget);
        using var fileWorkspace = TemporaryWorkspace.Create("mutation-lock-file");
        fileWorkspace.CreateFile(".agents", "not-a-directory");
        using var directoryTargetWorkspace = TemporaryWorkspace.Create("mutation-lock-directory-target");
        directoryTargetWorkspace.CreateDirectory(WorkspaceLockRequest.RelativePath);
        var manager = new WorkspaceLockManager(new PhysicalPathResolver());

        var alias = await manager.AcquireAsync(
            Request(Workspace(aliasWorkspace), "index"),
            TestContext.Current.CancellationToken);
        var file = await manager.AcquireAsync(
            Request(Workspace(fileWorkspace), "index"),
            TestContext.Current.CancellationToken);
        var fileAlias = await manager.AcquireAsync(
            Request(Workspace(fileAliasWorkspace), "index"),
            TestContext.Current.CancellationToken);
        var internalAlias = await manager.AcquireAsync(
            Request(Workspace(internalAliasWorkspace), "index"),
            TestContext.Current.CancellationToken);
        var internalFileAlias = await manager.AcquireAsync(
            Request(Workspace(internalFileAliasWorkspace), "index"),
            TestContext.Current.CancellationToken);
        var directoryTarget = await manager.AcquireAsync(
            Request(Workspace(directoryTargetWorkspace), "index"),
            TestContext.Current.CancellationToken);

        Assert.Equal(WorkspaceLockState.Failed, alias.State);
        Assert.Equal(FilesystemFailureKind.InvalidPath, alias.Failure?.Kind);
        Assert.Equal(WorkspaceLockState.Failed, file.State);
        Assert.Equal(FilesystemFailureKind.InvalidPath, file.Failure?.Kind);
        Assert.Equal(WorkspaceLockState.Failed, fileAlias.State);
        Assert.Equal(FilesystemFailureKind.InvalidPath, fileAlias.Failure?.Kind);
        var internalAliasLease = Assert.IsType<WorkspaceLockLease>(internalAlias.Lease);
        Assert.Equal(WorkspaceLockState.Acquired, internalAlias.State);
        Assert.Equal(WorkspaceLockState.Failed, internalFileAlias.State);
        Assert.Equal(FilesystemFailureKind.InvalidPath, internalFileAlias.Failure?.Kind);
        Assert.Equal(WorkspaceLockState.Failed, directoryTarget.State);
        Assert.Equal(FilesystemFailureKind.InvalidPath, directoryTarget.Failure?.Kind);
        Assert.False(File.Exists(outside.Combine("open-forge.lock")));
        Assert.Equal("retained", await File.ReadAllTextAsync(
            internalTarget,
            TestContext.Current.CancellationToken));
        Assert.Equal("retained", await File.ReadAllTextAsync(
            internalFileTarget,
            TestContext.Current.CancellationToken));
        Assert.Equal("outside", await File.ReadAllTextAsync(
            outsideLock,
            TestContext.Current.CancellationToken));
        await internalAliasLease.DisposeAsync();
        File.Delete(internalAliasWorkspace.Combine("lock-container/open-forge.lock"));
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
